using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;

using AquaScale.Api.Data; 
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Services;
using AquaScale.Api.Authorization;
using AquaScale.Api.Models.Mirror;
using AquaScale.Api.Services.Ocr;
using AquaScale.Api.Services.Storage;


var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// DATABASE & CORE SERVICES
// ============================================================================
builder.Services.AddDbContext<AquaScaleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseDb"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddMemoryCache();
builder.Services.AddScoped<PropertyOwnershipService>();
builder.Services.AddScoped<BuyerContactService>();
builder.Services.AddHttpClient<IOcrService, GoogleVisionOcrService>();
builder.Services.AddScoped<ReadingValidationService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IPhotoStorageService, R2StorageService>();

// ============================================================================
// CORS CONFIGURATION
// ============================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173") // TODO: confirm actual frontend dev port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // required — without this, the session cookie won't be sent/received cross-origin
});

// ============================================================================
// AUTHENTICATION
// ============================================================================
// Using session-cookie auth, per the decision to use ASP.NET's auth engine against the
// custom `profiles` table rather than default Identity schema / Supabase Auth. 
builder.Services.AddSingleton<IPasswordHasher<Profile>, PasswordHasher<Profile>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "aquascale_session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None; // None required for cross-site cookies, switch to Lax if same-site
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // requires HTTPS
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        // Override default redirect-to-login-page behavior with plain status codes for the API
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// ============================================================================
// AUTHORIZATION
// ============================================================================
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

// ============================================================================
// API, CONTROLLERS & OPENAPI
// ============================================================================
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // https://aka.ms/aspnet/openapi

// ============================================================================
// BUILD THE APPLICATION
// ============================================================================
var app = builder.Build();

// ============================================================================
// HTTP REQUEST PIPELINE (MIDDLEWARE)
// NOTE: Order is strictly enforced by ASP.NET Core!
// ============================================================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("Frontend"); // Must be before Auth
app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();
app.MapControllers();

// ============================================================================
// 7. ENDPOINTS & ROUTES
// ============================================================================

if (app.Environment.IsDevelopment())
{
    app.MapPost("/dev/seed-admin", async (AquaScaleDbContext db, IPasswordHasher<Profile> hasher, IConfiguration config) =>
    {
        const string devEmail = "admin@aquascale.dev";
        var devPassword = config["DevSeed:AdminPassword"] 
            ?? throw new InvalidOperationException("DevSeed:AdminPassword not configured.");

        if (await db.Profiles.AnyAsync(p => p.Email == devEmail))
            return Results.Conflict("Dev admin already seeded.");

        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null)
        {
            adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin", IsSystem = true, Position = 0 };
            db.Roles.Add(adminRole);
        }

        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            RoleId = adminRole.Id,
            Role = adminRole,
            FullName = "Dev Admin",
            Email = devEmail,
            IsActive = true,
            MustChangePassword = false,
            PasswordHash = string.Empty, 
        };
        profile.PasswordHash = hasher.HashPassword(profile, devPassword);

        db.Profiles.Add(profile);
        await db.SaveChangesAsync();

        return Results.Ok(new { email = devEmail, password = devPassword });
    });

    app.MapPost("/dev/seed-buyer", async (AquaScaleDbContext db, IPasswordHasher<Profile> hasher, IConfiguration config, string buyerId) =>
    {
        var mirrorBuyer = await db.Set<MirrorBuyer>().FirstOrDefaultAsync(b => b.BuyerId == buyerId);
        if (mirrorBuyer is null)
            return Results.NotFound($"No mirror_buyer found with BuyerId '{buyerId}'.");

        var buyerRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Buyer");
        if (buyerRole is null)
            return Results.NotFound("Customer/Buyer role not seeded yet.");

        var testEmail = $"buyer-{buyerId.ToLower()}@aquascale.dev";
        var devPassword = config["DevSeed:AdminPassword"]!;

        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            RoleId = buyerRole.Id,
            Role = buyerRole,
            FullName = $"Test Buyer ({buyerId})",
            Email = testEmail,
            BuyerRef = mirrorBuyer.Id,
            IsActive = true,
            MustChangePassword = true,
            PasswordHash = string.Empty,
        };
        profile.PasswordHash = hasher.HashPassword(profile, devPassword);

        db.Profiles.Add(profile);
        await db.SaveChangesAsync();

        return Results.Ok(new { email = testEmail, password = devPassword, linkedTo = buyerId });
    });

    app.MapGet("/dev/test-buyer-contact", async (BuyerContactService svc, string buyerId) =>
    {
        var contact = await svc.ResolveContactAsync(buyerId);
        var canIssue = await svc.CanIssueCredentialsAsync(buyerId);
        return Results.Ok(new { buyerId, contact.MobileNo, contact.Email, canIssue });
    });

    app.MapPost("/dev/test-ocr", async (IOcrService ocr, IFormFile image) =>
    {
        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);
        var result = await ocr.ReadMeterImageAsync(ms.ToArray());

        // Extract the actual numeric value from the raw OCR text
        var numericReading = GoogleVisionOcrService.ExtractNumericReading(result.RawText);

        return Results.Ok(new
        {
            result.Success,
            result.RawText,
            NumericReading = numericReading, // The actual happy-path proof
            result.OverallConfidence,
            DigitCount = result.Digits?.Count ?? 0,
            result.ErrorMessage,
        });
    }).DisableAntiforgery();

    app.MapPost("/dev/test-validation", async (
    ReadingValidationService validator,
    Guid meterId,
    decimal rawReading,
    float confidence) =>
    {
        var result = await validator.ValidateAsync(meterId, rawReading, confidence);
        return Results.Ok(result);
    });

    app.MapGet("/dev/test-qrcode", async (IQrCodeService qr, Guid meterId) =>
    {
        var pngBytes = qr.GenerateQrCode(meterId);
        return Results.File(pngBytes, "image/png");
    });
    
}
// Seeder
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AquaScaleDbContext>());
}


// ============================================================================
// RUN APPLICATION
// ============================================================================
app.Run();

// ============================================================================
// RECORDS / CLASSES
// ============================================================================
