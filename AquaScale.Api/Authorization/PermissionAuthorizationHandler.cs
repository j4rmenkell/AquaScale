using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AquaScale.Api.Data;
using System.Security.Claims;

namespace AquaScale.Api.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    private readonly AquaScaleDbContext _db;
    private readonly IMemoryCache _cache;

    // Short TTL, not manual invalidation — a role's permission set may change
    // (Admin edits role_permissions); this bounds staleness to ~90s instead of
    // requiring cache-busting plumbing on every edit path.
    private static readonly TimeSpan RolePermissionCacheTtl = TimeSpan.FromSeconds(90);

    public PermissionAuthorizationHandler(AquaScaleDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizationRequirement requirement)
    {
        var idClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var profileId))
        {
            return; // no valid identity — requirement not met, falls through to 403
        }

        // Fresh lookup every request — deliberately uncached, so a deactivated
        // or reassigned profile can't ride an old cached role for its cache window.
        var profile = await _db.Profiles
            .AsNoTracking()
            .Select(p => new { p.Id, p.RoleId, p.IsActive })
            .FirstOrDefaultAsync(p => p.Id == profileId);

        if (profile is null || !profile.IsActive)
        {
            return;
        }

        var permissionKeys = await GetRolePermissionKeysAsync(profile.RoleId);

        if (permissionKeys.Contains(requirement.PermissionKey))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<HashSet<string>> GetRolePermissionKeysAsync(Guid roleId)
    {
        var cacheKey = $"role-permissions:{roleId}";

        if (_cache.TryGetValue(cacheKey, out HashSet<string>? cached) && cached is not null)
        {
            return cached;
        }

        var keys = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Key)
            .ToListAsync();

        var keySet = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);
        _cache.Set(cacheKey, keySet, RolePermissionCacheTtl);
        return keySet;
    }
}