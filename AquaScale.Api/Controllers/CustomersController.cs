using AquaScale.Api.Authorization;
using AquaScale.Api.Data;
using AquaScale.Api.Models.Dtos;
using AquaScale.Api.Models.Mirror;
using AquaScale.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaScale.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AquaScaleDbContext _db;
    private readonly PropertyOwnershipService _ownership;

    public CustomersController(AquaScaleDbContext db, PropertyOwnershipService ownership)
    {
        _db = db;
        _ownership = ownership;
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.CustomersView)]
    public async Task<ActionResult<PagedResult<CustomerListItemDto>>> GetList(
        [FromQuery] Guid? subdivisionId,
        [FromQuery] string? search,
        [FromQuery] string? utilityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query =
            from m in _db.Meters
            join p in _db.Properties on m.PropertyId equals p.Id
            join s in _db.Subdivisions on p.SubdivisionId equals s.Id
            join mam in _db.Set<MirrorAccountMeter>() on m.MirrorAcctmtrId equals mam.Id into mamJoin
            from mam in mamJoin.DefaultIfEmpty()
            select new { m, p, s, mam };

        if (subdivisionId.HasValue)
            query = query.Where(x => x.s.Id == subdivisionId.Value);
        if (!string.IsNullOrWhiteSpace(utilityType))
            query = query.Where(x => x.m.UtilityType == utilityType);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => (x.p.Block ?? "").Contains(search) || (x.p.Lot ?? "").Contains(search) || (x.mam.AccountNo ?? "").Contains(search));

        var totalCount = await query.CountAsync(ct);

        var page_ = await query
            .OrderBy(x => x.s.Name).ThenBy(x => x.p.Block).ThenBy(x => x.p.Lot)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new { x.m.Id, x.s.Name, AccountNo = x.mam.AccountNo, x.p.Block, x.p.Lot, x.p.CompPbl, x.m.UtilityType, MeterStatusCode = x.mam.MeterStatus })
            .ToListAsync(ct);

        // Small page sizes (10-25) — per-row homeowner resolution here is an
        // accepted N+1 tradeoff, not a bulk-query optimization concern yet.
        var items = new List<CustomerListItemDto>();
        foreach (var row in page_)
        {
            var buyerName = await ResolveBuyerNameAsync(row.CompPbl, ct);
            var accountStatus = buyerName is not null ? "Active" : "New Customer"; // ASSUMPTION — confirm

            items.Add(new CustomerListItemDto(
                row.Id, row.Name, row.AccountNo, row.Block, row.Lot,
                buyerName, accountStatus, row.UtilityType, row.MeterStatusCode ?? "", null));
        }

        return Ok(new PagedResult<CustomerListItemDto>(items, totalCount, page, pageSize));
    }

    [HttpGet("{meterId}/ledger")]
    [RequirePermission(PermissionKeys.CustomersView)]
    public async Task<IActionResult> GetLedger(Guid meterId, CancellationToken ct)
    {
        var meter = await _db.Meters.FindAsync(new object[] { meterId }, ct);
        if (meter is null) return NotFound();

        var property = await _db.Properties.FindAsync(new object[] { meter.PropertyId }, ct);
        var subdivision = property is null ? null : await _db.Subdivisions.FindAsync(new object[] { property.SubdivisionId }, ct);
        var mam = meter.MirrorAcctmtrId is null ? null : await _db.Set<MirrorAccountMeter>().FindAsync(new object[] { meter.MirrorAcctmtrId }, ct);

        var buyerName = property is null ? null : await ResolveBuyerNameAsync(property.CompPbl, ct);

        var consumption = mam is null ? new() : await _db.Set<MirrorConsumption>()
            .Where(c => c.AcctMtrId == mam.Id)
            .OrderByDescending(c => c.DateRead)
            .Take(20)
            .ToListAsync(ct);

        var payments = mam is null ? new() : await _db.Set<MirrorPayment>()
            .Where(pm => pm.AcctMtrId == mam.Id)
            .OrderByDescending(pm => pm.OrDate)
            .Take(20)
            .ToListAsync(ct);

        return Ok(new
        {
            Customer = new { AccountNo = mam?.AccountNo, Name = buyerName, Status = buyerName is not null ? "Active" : "New Customer" },
            Property = new { Subdivision = subdivision?.Name, property?.Block, property?.Lot },
            Meter = new { meter.UtilityType, meter.QrCode, MeterStatusCode = mam?.MeterStatus },
            Consumption = consumption,
            Payments = payments,
            // Others, OtherCharges, Logs — deferred pending WEBS table investigation
        });
    }

    private async Task<string?> ResolveBuyerNameAsync(string? compPbl, CancellationToken ct)
    {
        if (compPbl is null) return null;

        var reservation = await _db.Set<MirrorReservation>()
            .Where(r => r.CompPbl == compPbl && r.BackoutType == null)
            .OrderByDescending(r => r.DateReserved)
            .FirstOrDefaultAsync(ct);

        if (reservation?.BuyerId is null) return null;

        var buyer = await _db.Set<MirrorBuyer>()
            .FirstOrDefaultAsync(b => b.BuyerId == reservation.BuyerId, ct);

        return buyer?.BuyerName;
    }
}