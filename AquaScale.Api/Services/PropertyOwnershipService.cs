using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Data;
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Models.Mirror;

namespace AquaScale.Api.Services;

public class PropertyOwnershipService
{
    private readonly AquaScaleDbContext _context;

    public PropertyOwnershipService(AquaScaleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Resolves the current homeowner for a property by walking the
    /// mirror_reservations -> mirror_buyer -> profiles chain.
    /// 
    /// Ownership is NOT a stored column on Property — mirror_reservations
    /// is a history table (confirmed: up to 9 rows per lot). The active
    /// reservation is the one with backout_type IS NULL (confirmed against
    /// real WEBS data + M_GenCodes Group 23, which decodes backout_type
    /// as a reason code, only ever populated on superseded reservations).
    /// </summary>
    public async Task<Profile?> GetCurrentHomeownerAsync(Guid propertyId, CancellationToken ct = default)
    {
        var property = await _context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propertyId, ct);

        if (property?.CompPbl is null)
            return null;

        var activeReservation = await _context.Set<MirrorReservation>()
            .AsNoTracking()
            .Where(r => r.CompPbl == property.CompPbl && r.BackoutType == null)
            .OrderByDescending(r => r.DateReserved)
            .FirstOrDefaultAsync(ct);

        if (activeReservation?.BuyerId is null)
            return null;

        var mirrorBuyer = await _context.Set<MirrorBuyer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BuyerId == activeReservation.BuyerId, ct);

        if (mirrorBuyer is null)
            return null;

        var profile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.BuyerRef == mirrorBuyer.Id, ct);

        return profile;
    }
}