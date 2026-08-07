using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Authorization;

namespace AquaScale.Api.Data;

public static class DataSeeder
{
    private static readonly string[] RoleNames = 
        { "Admin", "Authorized Person", "Field Worker", "Accounting", "Buyer" };

    private static readonly (string Key, string Category, string Description)[] Permissions =
    {
        (PermissionKeys.EmployeesCreate, "Administration", "Create an employee account"),
        (PermissionKeys.RolesView, "Administration", "View system roles"),
        (PermissionKeys.RolesManage, "Administration", "Manage role permissions"),
        (PermissionKeys.CustomersView, "Customers", "View customer directory and ledgers"),
        (PermissionKeys.PropertiesView, "Customers", "View property details"),
        (PermissionKeys.MeterReadingsCreate, "Field Operations", "Capture a new meter reading"),
        (PermissionKeys.PaymentsVerify, "Accounting", "Verify a submitted payment proof"),
        (PermissionKeys.ServiceRequestsManage, "Service Requests", "Manage homeowner service requests")
    };

    // Only map non-admin roles here. Admin gets everything automatically below.
    private static readonly (string Role, string PermissionKey)[] SpecificRoleGrants =
    {
        ("Accounting", PermissionKeys.PaymentsVerify),
        ("Authorized Person", PermissionKeys.PaymentsVerify),
        ("Field Worker", PermissionKeys.MeterReadingsCreate),
        ("Field Worker", PermissionKeys.ServiceRequestsManage)
    };

    public static async Task SeedAsync(AquaScaleDbContext db)
    {
        // 1. Seed Roles
        foreach (var name in RoleNames)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name))
                db.Roles.Add(new Role { Id = Guid.NewGuid(), Name = name, IsSystem = true, Position = 0 });
        }
        await db.SaveChangesAsync();

        // 2. Seed Permissions
        foreach (var (key, category, description) in Permissions)
        {
            if (!await db.Permissions.AnyAsync(p => p.Key == key))
                db.Permissions.Add(new Permission { Id = Guid.NewGuid(), Key = key, Category = category, Description = description });
        }
        await db.SaveChangesAsync();

        // 3. AUTO-GRANT EVERYTHING TO ADMIN
        var adminRole = await db.Roles.FirstAsync(r => r.Name == "Admin");
        var allPermissions = await db.Permissions.ToListAsync();
        var adminExistingGrants = await db.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        foreach (var perm in allPermissions)
        {
            if (!adminExistingGrants.Contains(perm.Id))
            {
                db.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
            }
        }

        // 4. Grant specific permissions to other roles
        foreach (var (roleName, permKey) in SpecificRoleGrants)
        {
            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            var perm = await db.Permissions.FirstOrDefaultAsync(p => p.Key == permKey);

            if (role == null || perm == null) continue;

            var exists = await db.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id);
            if (!exists)
                db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id });
        }

        await db.SaveChangesAsync();
    }
}