// Data/DataSeeder.cs
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
        (PermissionKeys.EmployeesCreate, "Employees", "Create an employee account"),
        (PermissionKeys.PaymentsVerify, "Payments", "Verify a submitted payment proof"),
        (PermissionKeys.RolesView, "Roles", "Viewing of roles"),
        (PermissionKeys.RolesManage, "Roles", "Managing of roles"),
        // add here as new permissions get built
    };

    private static readonly (string Role, string PermissionKey)[] RoleGrants =
    {
        ("Admin", PermissionKeys.EmployeesCreate),
        ("Accounting", PermissionKeys.PaymentsVerify),
        ("Authorized Person", PermissionKeys.PaymentsVerify),
        ("Admin", PermissionKeys.RolesView),
        ("Admin", PermissionKeys.RolesManage),
        // add here as new permissions get built
    };

    public static async Task SeedAsync(AquaScaleDbContext db)
    {
        foreach (var name in RoleNames)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name))
                db.Roles.Add(new Role { Id = Guid.NewGuid(), Name = name, IsSystem = true });
        }
        await db.SaveChangesAsync();

        foreach (var (key, category, description) in Permissions)
        {
            if (!await db.Permissions.AnyAsync(p => p.Key == key))
                db.Permissions.Add(new Permission { Id = Guid.NewGuid(), Key = key, Category = category, Description = description });
        }
        await db.SaveChangesAsync();

        foreach (var (roleName, permKey) in RoleGrants)
        {
            var role = await db.Roles.FirstAsync(r => r.Name == roleName);
            var perm = await db.Permissions.FirstAsync(p => p.Key == permKey);

            var exists = await db.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id);
            if (!exists)
                db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id });
        }
        await db.SaveChangesAsync();
    }
}