namespace AquaScale.Api.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string RoleName { get; set; } = null!;
    public bool MustChangePassword { get; set; }
}