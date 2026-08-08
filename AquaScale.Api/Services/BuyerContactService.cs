using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Data;

namespace AquaScale.Api.Services;

public class BuyerContactService
{
    private readonly WebsDbContext _webs;

    public BuyerContactService(WebsDbContext webs)
    {
        _webs = webs;
    }

    public record ResolvedContact(string? MobileNo, string? Email);

    public async Task<ResolvedContact> ResolveContactAsync(string buyerId, CancellationToken ct = default)
    {
        // buyerId is char(8) in WEBS — Trim() guards against any trailing spaces.
        // WEBS stores these as fixed-width char; the DbContext column type mapping
        // means EF Core sends the exact value to SQL Server without padding.
        var cleanId = buyerId.Trim();

        var mobile = await _webs.BuyerContacts
            .Where(c => c.BuyerId == cleanId && c.MobileNo != null && c.MobileNo != "")
            .OrderByDescending(c => c.DateUpdated)
            .Select(c => c.MobileNo)
            .FirstOrDefaultAsync(ct);

        var email = await _webs.BuyerContacts
            .Where(c => c.BuyerId == cleanId && c.Email != null && c.Email != "")
            .OrderByDescending(c => c.DateUpdated)
            .Select(c => c.Email)
            .FirstOrDefaultAsync(ct);

        return new ResolvedContact(mobile, email);
    }

    /// <summary>
    /// Returns true if the buyer has at least one non-empty mobile or email on record.
    /// Used by seed-buyer and future onboarding flows to check if credentials can be issued.
    /// </summary>
    public async Task<bool> CanIssueCredentialsAsync(string buyerId, CancellationToken ct = default)
    {
        var cleanId = buyerId.Trim();

        return await _webs.BuyerContacts
            .AnyAsync(c => c.BuyerId == cleanId &&
                           (c.MobileNo != null && c.MobileNo != "" ||
                            c.Email    != null && c.Email    != ""), ct);
    }
}