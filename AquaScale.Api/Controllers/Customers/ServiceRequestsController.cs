using System.Security.Claims;
using AquaScale.Api.Authorization;
using AquaScale.Api.Data;
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Models.Dtos;
using AquaScale.Api.Models.Webs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaScale.Api.Controllers.Customers;

[ApiController]
[Route("api/service-requests")]
[RequirePermission(PermissionKeys.CustomersView)]
public class ServiceRequestsController : ControllerBase
{
    private readonly AquaScaleDbContext _db;
    private readonly WebsDbContext      _webs;

    public ServiceRequestsController(AquaScaleDbContext db, WebsDbContext webs)
    {
        _db   = db;
        _webs = webs;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ServiceRequestListItemDto>>> GetList(
        [FromQuery] string? search,
        [FromQuery] string? issueType,
        [FromQuery] string? status,
        [FromQuery] bool?   assignedToMe,
        [FromQuery] Guid?   assignedTo,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10,
        CancellationToken   ct       = default)
    {
        var query = _db.ServiceRequests
            .Include(sr => sr.Property)
                .ThenInclude(p => p.Subdivision)
            .Include(sr => sr.Meter)
            .Include(sr => sr.Customer)
            .Include(sr => sr.AssignedToProfile)
                .ThenInclude(p => p!.Role)
            .AsQueryable();

        // Filter by assigned user
        if (assignedToMe == true)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(idClaim, out var currentUserId))
            {
                query = query.Where(sr => sr.AssignedTo == currentUserId);
            }
        }
        else if (assignedTo.HasValue)
        {
            query = query.Where(sr => sr.AssignedTo == assignedTo.Value);
        }

        // Filter by issue type
        if (!string.IsNullOrWhiteSpace(issueType))
        {
            query = query.Where(sr => sr.IssueType.ToLower() == issueType.Trim().ToLower());
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(sr => sr.Status.ToLower() == status.Trim().ToLower());
        }

        var totalCount = await query.CountAsync(ct);

        var requests = await query
            .OrderByDescending(sr => sr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = new List<ServiceRequestListItemDto>();

        foreach (var sr in requests)
        {
            var accountNo = await ResolveAccountNumberAsync(sr, ct);
            var accountName = await ResolveAccountNameAsync(sr, ct);

            // Apply search filter in-memory if requested (across Account, Name, Block, Lot, IssueType)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                var matchAccount = accountNo?.ToLower().Contains(term) ?? false;
                var matchName    = accountName?.ToLower().Contains(term) ?? false;
                var matchBlock   = sr.Property.Block?.ToLower().Contains(term) ?? false;
                var matchLot     = sr.Property.Lot?.ToLower().Contains(term) ?? false;
                var matchIssue   = sr.IssueType.ToLower().Contains(term);

                if (!matchAccount && !matchName && !matchBlock && !matchLot && !matchIssue)
                {
                    continue;
                }
            }

            var meterType = sr.Meter?.UtilityType ?? "Water";
            var assignedToDto = sr.AssignedToProfile is not null
                ? new AssignedToDto(
                    sr.AssignedToProfile.Id,
                    sr.AssignedToProfile.FullName,
                    sr.AssignedToProfile.Role?.Name)
                : null;

            items.Add(new ServiceRequestListItemDto(
                sr.Id,
                accountNo,
                accountName,
                sr.Property.Subdivision.Name,
                sr.Property.Block,
                sr.Property.Lot,
                sr.IssueType,
                meterType,
                sr.Status,
                assignedToDto,
                sr.CreatedAt,
                !string.IsNullOrWhiteSpace(sr.ImageUrl),
                sr.Description,
                sr.InternalNotes
            ));
        }

        return Ok(new PagedResult<ServiceRequestListItemDto>(items, totalCount, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceRequestDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var sr = await _db.ServiceRequests
            .Include(r => r.Property)
                .ThenInclude(p => p.Subdivision)
            .Include(r => r.Meter)
            .Include(r => r.Customer)
            .Include(r => r.AssignedToProfile)
                .ThenInclude(p => p!.Role)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (sr is null)
        {
            return NotFound(new { message = $"Service request with ID '{id}' was not found." });
        }

        var accountNo   = await ResolveAccountNumberAsync(sr, ct);
        var accountName = await ResolveAccountNameAsync(sr, ct);
        var meterType   = sr.Meter?.UtilityType ?? "Water";

        var assignedToDto = sr.AssignedToProfile is not null
            ? new AssignedToDto(
                sr.AssignedToProfile.Id,
                sr.AssignedToProfile.FullName,
                sr.AssignedToProfile.Role?.Name)
            : null;

        var dto = new ServiceRequestDetailDto(
            sr.Id,
            sr.PropertyId,
            sr.MeterId,
            sr.CustomerId,
            accountNo,
            accountName,
            sr.Property.Subdivision.Name,
            sr.Property.Block,
            sr.Property.Lot,
            sr.Property.CompPbl,
            sr.Customer?.ContactNo,
            sr.Customer?.Email,
            sr.IssueType,
            meterType,
            sr.Status,
            assignedToDto,
            sr.CreatedAt,
            sr.ResolvedAt,
            !string.IsNullOrWhiteSpace(sr.ImageUrl),
            sr.ImageUrl,
            sr.Description,
            sr.InternalNotes
        );

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequestDetailDto>> Create(
        [FromBody] CreateServiceRequestDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.IssueType))
        {
            return BadRequest(new { message = "IssueType is required." });
        }

        var propertyExists = await _db.Properties.AnyAsync(p => p.Id == dto.PropertyId, ct);
        if (!propertyExists)
        {
            return BadRequest(new { message = $"Property with ID '{dto.PropertyId}' does not exist." });
        }

        var sr = new ServiceRequest
        {
            Id          = Guid.NewGuid(),
            PropertyId  = dto.PropertyId,
            MeterId     = dto.MeterId,
            CustomerId  = dto.CustomerId,
            IssueType   = dto.IssueType.Trim(),
            Description = dto.Description,
            ImageUrl    = dto.ImageUrl,
            Status      = "Open",
            CreatedAt   = DateTime.UtcNow
        };

        _db.ServiceRequests.Add(sr);
        await _db.SaveChangesAsync(ct);

        return await GetById(sr.Id, ct);
    }

    [HttpPut("{id}/assignment")]
    public async Task<IActionResult> UpdateAssignment(
        Guid id,
        [FromBody] UpdateServiceRequestAssignmentDto dto,
        CancellationToken ct)
    {
        var sr = await _db.ServiceRequests
            .Include(r => r.AssignedToProfile)
                .ThenInclude(p => p!.Role)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (sr is null)
        {
            return NotFound(new { message = $"Service request with ID '{id}' was not found." });
        }

        if (dto.AssignedToProfileId.HasValue)
        {
            var profile = await _db.Profiles
                .Include(p => p.Role)
                .FirstOrDefaultAsync(p => p.Id == dto.AssignedToProfileId.Value && p.IsActive, ct);

            if (profile is null)
            {
                return BadRequest(new { message = $"Active profile with ID '{dto.AssignedToProfileId.Value}' was not found." });
            }

            sr.AssignedTo = profile.Id;

            // Transition from Open to In Progress on assignment
            if (sr.Status.Equals("Open", StringComparison.OrdinalIgnoreCase))
            {
                sr.Status = "In Progress";
            }
        }
        else
        {
            sr.AssignedTo = null;
        }

        if (dto.InternalNotes is not null)
        {
            sr.InternalNotes = dto.InternalNotes;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            sr.Id,
            sr.AssignedTo,
            sr.Status,
            sr.InternalNotes
        });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateServiceRequestStatusDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            return BadRequest(new { message = "Status is required." });
        }

        var sr = await _db.ServiceRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (sr is null)
        {
            return NotFound(new { message = $"Service request with ID '{id}' was not found." });
        }

        var normalizedStatus = dto.Status.Trim();
        sr.Status = normalizedStatus;

        if (normalizedStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
        {
            sr.ResolvedAt = DateTime.UtcNow;
        }
        else
        {
            sr.ResolvedAt = null;
        }

        if (dto.InternalNotes is not null)
        {
            sr.InternalNotes = dto.InternalNotes;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            sr.Id,
            sr.Status,
            sr.ResolvedAt,
            sr.InternalNotes
        });
    }

    // ── Helper methods for cross-database resolution ─────────────────────────

    private async Task<string?> ResolveAccountNumberAsync(ServiceRequest sr, CancellationToken ct)
    {
        // 1. Direct from Meter's MirrorAcctmtrId in WEBS T_Account_Meter
        if (sr.Meter?.MirrorAcctmtrId.HasValue == true)
        {
            var wam = await _webs.AccountMeters
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == sr.Meter.MirrorAcctmtrId.Value, ct);

            if (!string.IsNullOrWhiteSpace(wam?.AccountNo))
                return wam.AccountNo.Trim();
        }

        // 2. From Property's MirrorAccountNo
        if (!string.IsNullOrWhiteSpace(sr.Property.MirrorAccountNo))
        {
            return sr.Property.MirrorAccountNo.Trim();
        }

        // 3. Fallback: inspect any Meter associated with the Property
        var propertyMeter = await _db.Meters
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.PropertyId == sr.PropertyId && m.MirrorAcctmtrId != null, ct);

        if (propertyMeter?.MirrorAcctmtrId.HasValue == true)
        {
            var wam = await _webs.AccountMeters
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == propertyMeter.MirrorAcctmtrId.Value, ct);

            if (!string.IsNullOrWhiteSpace(wam?.AccountNo))
                return wam.AccountNo.Trim();
        }

        return null;
    }

    private async Task<string?> ResolveAccountNameAsync(ServiceRequest sr, CancellationToken ct)
    {
        // 1. Resolve via WEBS reservation and buyer tables
        if (!string.IsNullOrWhiteSpace(sr.Property.CompPbl))
        {
            var reservation = await _webs.Reservations
                .AsNoTracking()
                .Where(r => r.CompPBL == sr.Property.CompPbl && r.BackoutType == null)
                .OrderByDescending(r => r.DateReserved)
                .FirstOrDefaultAsync(ct);

            if (reservation?.BuyerId is not null)
            {
                var buyer = await _webs.Buyers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BuyerId == reservation.BuyerId.Trim(), ct);

                if (buyer is not null)
                {
                    return !string.IsNullOrWhiteSpace(buyer.BuyerName)
                        ? buyer.BuyerName.Trim()
                        : $"{buyer.LastName}, {buyer.FirstName}".Trim();
                }
            }
        }

        // 2. Fallback to Customer Profile FullName
        return sr.Customer?.FullName;
    }
}
