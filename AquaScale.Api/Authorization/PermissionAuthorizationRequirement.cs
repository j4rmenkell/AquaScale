using Microsoft.AspNetCore.Authorization;

namespace AquaScale.Api.Authorization;

public class PermissionAuthorizationRequirement : IAuthorizationRequirement
{
    public string PermissionKey { get; }

    public PermissionAuthorizationRequirement(string permissionKey)
    {
        PermissionKey = permissionKey;
    }
}