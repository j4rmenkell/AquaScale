using AquaScale.Api.Authorization;
using AquaScale.Api.Data;
using AquaScale.Api.Models.Dtos;
using AquaScale.Api.Models.Webs;
using AquaScale.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaScale.Api.Controllers.BackOffice.Customers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AquaScaleDbContext  _db;
    private readonly WebsDbContext       _webs;
    private readonly PropertyOwnershipService _ownership;

    public CustomersController(AquaScaleDbContext db, WebsDbContext webs, PropertyOwnershipService ownership)
    {
        _db        = db;
        _webs      = webs;
        _ownership = ownership;
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.CustomersView)]
    public async Task<ActionResult<PagedResult<CustomerListItemDto>>> GetList(
        [FromQuery] Guid?   subdivisionId,
        [FromQuery] string? search,
        [FromQuery] string? utilityType,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10,
        CancellationToken   ct       = default)
    {
        var query =
            from m in _db.Meters
            join p in _db.Properties   on m.PropertyId     equals p.Id
            join s in _db.Subdivisions on p.SubdivisionId  equals s.Id
            select new { m, p, s };

        if (subdivisionId.HasValue)
            query = query.Where(x => x.s.Id == subdivisionId.Value);
        if (!string.IsNullOrWhiteSpace(utilityType))
            query = query.Where(x => x.m.UtilityType == utilityType);

        var totalCount = await query.CountAsync(ct);

        var rows = await query
            .OrderBy(x => x.s.Name).ThenBy(x => x.p.Block).ThenBy(x => x.p.Lot)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new
            {
                x.m.Id,
                SubdivisionName = x.s.Name,
                x.p.Block,
                x.p.Lot,
                x.p.CompPbl,
                x.m.UtilityType,
                x.m.MirrorAcctmtrId
            })
            .ToListAsync(ct);

        // Resolve WEBS account details per row (small page size — N+1 is acceptable here)
        var items = new List<CustomerListItemDto>();
        foreach (var row in rows)
        {
            WEBSAccountMeter? wam = null;
            if (row.MirrorAcctmtrId.HasValue)
                wam = await _webs.AccountMeters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == row.MirrorAcctmtrId.Value, ct);

            // Apply search filter against resolved AccountNo (WEBS side)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var block     = row.Block ?? "";
                var lot       = row.Lot ?? "";
                var accountNo = wam?.AccountNo ?? "";
                if (!block.Contains(search) && !lot.Contains(search) && !accountNo.Contains(search))
                    continue;
            }

            var buyerName     = await ResolveBuyerNameAsync(row.CompPbl, ct);
            var accountStatus = buyerName is not null ? "Active" : "New Customer";

            items.Add(new CustomerListItemDto(
                row.Id, row.SubdivisionName,
                wam?.AccountNo, row.Block, row.Lot,
                buyerName, accountStatus, row.UtilityType,
                wam?.MeterStatus?.Trim() ?? "", null));
        }

        return Ok(new PagedResult<CustomerListItemDto>(items, totalCount, page, pageSize));
    }

    [HttpGet("{meterId}/ledger")]
    [RequirePermission(PermissionKeys.CustomersView)]
    public async Task<IActionResult> GetLedger(Guid meterId, CancellationToken ct)
    {
        var meter = await _db.Meters.FindAsync(new object[] { meterId }, ct);
        if (meter is null) return NotFound();

        var property    = await _db.Properties.FindAsync(new object[] { meter.PropertyId }, ct);
        var subdivision = property is null ? null
            : await _db.Subdivisions.FindAsync(new object[] { property.SubdivisionId }, ct);

        WEBSAccountMeter? wam = null;
        if (meter.MirrorAcctmtrId.HasValue)
            wam = await _webs.AccountMeters
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == meter.MirrorAcctmtrId.Value, ct);

        var buyerName = property is null ? null : await ResolveBuyerNameAsync(property.CompPbl, ct);

        var consumption = wam is null ? new() : await _webs.Consumptions
            .AsNoTracking()
            .Where(c => c.AcctMtrId == wam.Id)
            .OrderByDescending(c => c.DateRead)
            .Take(20)
            .ToListAsync(ct);

        var payments = wam is null ? new() : await _webs.Payments
            .AsNoTracking()
            .Where(pm => pm.AcctMtrId == wam.Id)
            .OrderByDescending(pm => pm.OrDate)
            .Take(20)
            .ToListAsync(ct);

        return Ok(new
        {
            Customer    = new { AccountNo = wam?.AccountNo, Name = buyerName, Status = buyerName is not null ? "Active" : "New Customer" },
            Property    = new { Subdivision = subdivision?.Name, property?.Block, property?.Lot },
            Meter       = new { meter.UtilityType, meter.QrCode, MeterStatusCode = wam?.MeterStatus?.Trim() },
            Consumption = consumption,
            Payments    = payments,
        });
    }

    private async Task<string?> ResolveBuyerNameAsync(string? compPbl, CancellationToken ct)
    {
        if (compPbl is null) return null;

        var reservation = await _webs.Reservations
            .AsNoTracking()
            .Where(r => r.CompPBL == compPbl && r.BackoutType == null)
            .OrderByDescending(r => r.DateReserved)
            .FirstOrDefaultAsync(ct);

        if (reservation?.BuyerId is null) return null;

        var buyer = await _webs.Buyers
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BuyerId == reservation.BuyerId.Trim(), ct);

        // BuyerName is nullable in WEBS — fall back to LastName, FirstName
        return buyer?.BuyerName ?? (buyer is not null ? $"{buyer.LastName}, {buyer.FirstName}" : null);
    }
}
