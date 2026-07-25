namespace AquaScale.Api.Models.AquaScale;

public class Permission
{
    public Guid Id { get; set; }
    public string Key { get; set; } = null!;
    public string? Category { get; set; }
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}