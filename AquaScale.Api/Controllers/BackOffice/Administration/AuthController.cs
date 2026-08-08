using System.Security.Claims;
using AquaScale.Api.Data;
using AquaScale.Api.DTOs;
using AquaScale.Api.Models.AquaScale;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaScale.Api.Controllers.BackOffice.Administration;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AquaScaleDbContext _db;
    private readonly IPasswordHasher<Profile> _passwordHasher;

    public AuthController(AquaScaleDbContext db, IPasswordHasher<Profile> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        // Email is optional/nullable on Profile per the current model — case-insensitive match,
        // and explicitly excludes null emails so an empty request.Email can't match a profile
        // that has no email set.
        var profile = await _db.Profiles
            .Include(p => p.Role)
            .FirstOrDefaultAsync(p => p.Email != null && p.Email.ToLower() == request.Email.ToLower());

        // Deliberately identical error for "no such user" and "wrong password" — avoids
        // leaking which emails exist in the system via response differences.
        if (profile is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (!profile.IsActive)
        {
            return Unauthorized(new { message = "This account has been deactivated." });
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(profile, profile.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // PasswordVerificationResult.SuccessRehashNeeded means it matched, but the hash was
        // produced with an outdated algorithm/work-factor. Transparently upgrade it now.
        if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            profile.PasswordHash = _passwordHasher.HashPassword(profile, request.Password);
            await _db.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, profile.Id.ToString()),
            new(ClaimTypes.Name, profile.FullName),
            // Deliberately NOT including the role here. Per the existing RBAC design,
            // permissions are resolved dynamically per-request from role_permissions so
            // that edits apply immediately — baking the role into the cookie would mean
            // a mid-session role change stays stale until the user logs out and back in.
            // Role name is still returned in LoginResponse below for UI display only;
            // it must never be used to authorize anything.
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok(new LoginResponse
        {
            Id = profile.Id,
            FullName = profile.FullName,
            Email = profile.Email,
            RoleName = profile.Role.Name,
            MustChangePassword = profile.MustChangePassword,
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> Me()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var profileId))
        {
            return Unauthorized();
        }

        var profile = await _db.Profiles
            .Include(p => p.Role)
            .FirstOrDefaultAsync(p => p.Id == profileId);

        if (profile is null || !profile.IsActive)
        {
            // Profile was deleted or deactivated after the cookie was issued — the cookie
            // itself is still cryptographically valid, so we must check the DB, not just
            // trust the claims.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Unauthorized();
        }

        return Ok(new LoginResponse
        {
            Id = profile.Id,
            FullName = profile.FullName,
            Email = profile.Email,
            RoleName = profile.Role.Name,
            MustChangePassword = profile.MustChangePassword,
        });
    }
}
