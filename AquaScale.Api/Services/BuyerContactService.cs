using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Data;
using AquaScale.Api.Models.Mirror;

namespace AquaScale.Api.Services;

public class BuyerContactService
{
    private readonly AquaScaleDbContext _context;

    public BuyerContactService(AquaScaleDbContext context)
    {
        _context = context;
    }

    public record ResolvedContact(string? MobileNo, string? Email);

    public async Task<ResolvedContact> ResolveContactAsync(string buyerId, CancellationToken ct = default)
    {
        var mobile = await _context.MirrorBuyerContacts
            .Where(c => c.BuyerId == buyerId && c.MobileNo != null && c.MobileNo != "")
            .OrderByDescending(c => c.DateUpdated)
            .Select(c => c.MobileNo)
            .FirstOrDefaultAsync(ct);

        var email = await _context.MirrorBuyerContacts
            .Where(c => c.BuyerId == buyerId && c.Email != null && c.Email != "")
            .OrderByDescending(c => c.DateUpdated)
            .Select(c => c.Email)
            .FirstOrDefaultAsync(ct);

        return new ResolvedContact(mobile, email);
    }

    public async Task<bool> CanIssueCredentialsAsync(string buyerId, CancellationToken ct = default)
    {
        var contact = await ResolveContactAsync(buyerId, ct);
        return contact.MobileNo is not null || contact.Email is not null;
    }
}