using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Data;
using AquaScale.Api.Models.AquaScale;

namespace AquaScale.Api.Services;

public class PropertyOwnershipService
{
    private readonly AquaScaleDbContext _db;
    private readonly WebsDbContext      _webs;

    public PropertyOwnershipService(AquaScaleDbContext db, WebsDbContext webs)
    {
        _db   = db;
        _webs = webs;
    }

    /// <summary>
    /// Resolves the current homeowner for a property by walking the
    /// T_PM_Reservation → Profile chain.
    ///
    /// Ownership is NOT stored on Property — T_PM_Reservation is a history
    /// table with potentially many rows per lot. The active reservation is
    /// the one with BackoutType IS NULL (confirmed against real WEBS data).
    ///
    /// Profile.BuyerRef stores the WEBS Buyer_ID (char 8) directly,
    /// so no intermediate buyer lookup is needed.
    /// </summary>
    public async Task<Profile?> GetCurrentHomeownerAsync(Guid propertyId, CancellationToken ct = default)
    {
        var property = await _db.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propertyId, ct);

        if (property?.CompPbl is null)
            return null;

        // WEBS read: find the active reservation for this lot
        var activeReservation = await _webs.Reservations
            .AsNoTracking()
            .Where(r => r.CompPBL == property.CompPbl && r.BackoutType == null)
            .OrderByDescending(r => r.DateReserved)
            .FirstOrDefaultAsync(ct);

        if (activeReservation?.BuyerId is null)
            return null;

        // AquaScale read: find the buyer profile linked to this WEBS Buyer_ID
        var buyerId = activeReservation.BuyerId.Trim();
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.BuyerRef == buyerId, ct);

        return profile;
    }
}