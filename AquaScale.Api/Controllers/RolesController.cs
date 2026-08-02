using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Data;
using AquaScale.Api.Models.AquaScale; 
using AquaScale.Api.Authorization;
using AquaScale.Api.Models.Dtos;

namespace AquaScale.Api.Controllers;


[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly AquaScaleDbContext _db;
    public RolesController(AquaScaleDbContext db) => _db = db;

    [HttpGet]
    [RequirePermission(PermissionKeys.RolesView)]
    public async Task<IActionResult> GetRoles() =>
        Ok(await _db.Roles.Select(r => new { r.Id, r.Name, r.IsSystem, r.Color, r.Position }).ToListAsync());

    [HttpPost]
    [RequirePermission(PermissionKeys.RolesManage)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == request.Name))
            return Conflict("A role with this name already exists.");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            IsSystem = false, // <-- the field that matters most here
            Color = request.Color,
            Position = await _db.Roles.MaxAsync(r => (int?)r.Position) + 1 ?? 0,
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return Ok(role);
    }

    [HttpPut("{roleId}/permissions")]
    [RequirePermission(PermissionKeys.RolesManage)]
    public async Task<IActionResult> SetRolePermissions(Guid roleId, [FromBody] List<Guid> permissionIds)
    {
        var role = await _db.Roles.FindAsync(roleId);
        if (role is null) return NotFound();
        if (role.IsSystem) return StatusCode(403, "Cannot modify permissions of a system-defined role through this endpoint.");        // ^ worth deciding: do you want to allow editing the 5 defaults' permissions too?
        // If yes, remove this check. If you want the 5 defaults protected/fixed, keep it.

        var existing = _db.RolePermissions.Where(rp => rp.RoleId == roleId);
        _db.RolePermissions.RemoveRange(existing);

        foreach (var permId in permissionIds)
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId, GrantedAt = DateTime.UtcNow });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{roleId}")]
    [RequirePermission(PermissionKeys.RolesManage)]
    public async Task<IActionResult> DeleteRole(Guid roleId)
    {
        var role = await _db.Roles.FindAsync(roleId);
        if (role is null) return NotFound();
        if (role.IsSystem) return StatusCode(403, "Cannot delete a system-defined role.");
        if (await _db.Profiles.AnyAsync(p => p.RoleId == roleId))
            return Conflict("Cannot delete a role with active profiles assigned to it. Reassign them first.");

        // NEW: Clean up associated permissions before deleting the role
        var rolePermissions = _db.RolePermissions.Where(rp => rp.RoleId == roleId);
        _db.RolePermissions.RemoveRange(rolePermissions);

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}