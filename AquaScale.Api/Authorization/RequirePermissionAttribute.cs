using Microsoft.AspNetCore.Authorization;

namespace AquaScale.Api.Authorization;

/// <summary>
/// Usage: [RequirePermission("payments.verify")]
/// Maps to a dynamically-resolved policy — no need to pre-register every
/// permission key in Program.cs, since keys come from the permissions table,
/// not a fixed enum.
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(string permissionKey)
        : base(policy: $"{PolicyPrefix}{permissionKey}")
    {
    }
}