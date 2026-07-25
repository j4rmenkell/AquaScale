namespace AquaScale.Api.Models.AquaScale;

public class Profile
{
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string FullName { get; set; } = null!;
    public string? ContactNo { get; set; }
    public string? Email { get; set; }

    public Guid? BuyerRef { get; set; } // links to mirror_buyer for Customer/Buyer role

    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true; // forced first-login change
    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DateDeactivated { get; set; }
}