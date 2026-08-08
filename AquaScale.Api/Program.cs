using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;

using AquaScale.Api.Data; 
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Services;
using AquaScale.Api.Authorization;
using AquaScale.Api.Models.Webs;
using AquaScale.Api.Services.Ocr;
using AquaScale.Api.Services.Storage;


var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// DATABASE & CORE SERVICES
// ============================================================================
builder.Services.AddDbContext<AquaScaleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AquaScaleDb")));

// WEBS database — full read/write, no migrations ever.
// AquaScale.Api writes billing/consumption directly to WEBS through this context.
builder.Services.AddDbContext<WebsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WebsDb"),
        sqlOptions => sqlOptions.UseCompatibilityLevel(100)));

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

    app.MapPost("/dev/seed-buyer", async (
        AquaScaleDbContext db,
        IPasswordHasher<Profile> hasher,
        IConfiguration config,
        string buyerId,
        string? fullName = null) =>
    {
        // Dev-only: creates a Buyer-role profile linked to the given WEBS Buyer_ID.
        // Does NOT validate the buyerId against WEBS — WEBS views may not be set up yet.
        var buyerRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Buyer");
        if (buyerRole is null)
            return Results.NotFound("Buyer role not seeded yet. Run the API first so DataSeeder can create it.");

        var testEmail   = $"buyer-{buyerId.ToLower().Trim()}@aquascale.dev";
        var devPassword = config["DevSeed:AdminPassword"]!;

        var existing = await db.Profiles.FirstOrDefaultAsync(p => p.Email == testEmail);
        if (existing is not null)
            return Results.Conflict(new { message = $"A buyer profile for '{buyerId}' already exists.", email = testEmail });

        var profile = new Profile
        {
            Id                 = Guid.NewGuid(),
            RoleId             = buyerRole.Id,
            Role               = buyerRole,
            FullName           = fullName ?? $"Test Buyer ({buyerId})",
            Email              = testEmail,
            BuyerRef           = buyerId.Trim(),   // links to WEBS M_Buyer.Buyer_ID when views are ready
            IsActive           = true,
            MustChangePassword = true,
            PasswordHash       = string.Empty,
        };
        profile.PasswordHash = hasher.HashPassword(profile, devPassword);

        db.Profiles.Add(profile);
        await db.SaveChangesAsync();

        return Results.Ok(new { email = testEmail, password = devPassword, buyerRef = buyerId });
    });

    app.MapPost("/dev/seed-test-meter", async (Guid websMeterId, AquaScale.Api.Data.AquaScaleDbContext db) => 
    {
        var subId = Guid.NewGuid();
        db.Subdivisions.Add(new AquaScale.Api.Models.AquaScale.Subdivision { Id = subId, Name = "Test Subdivision" });
        
        var propId = Guid.NewGuid();
        db.Properties.Add(new AquaScale.Api.Models.AquaScale.Property { Id = propId, SubdivisionId = subId, Block = "1", Lot = "1" });
        
        db.Meters.Add(new AquaScale.Api.Models.AquaScale.Meter 
        { 
            Id = Guid.NewGuid(), 
            PropertyId = propId, 
            MirrorAcctmtrId = websMeterId, 
            UtilityType = "Water", 
            QrCode = "TEST-QR-001" 
        });
        
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Test meter and property seeded successfully!" });
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

// ============================================================================
// /dev/seed-subdivision
//
// FUNCTION: pulls real WEBS data (billing accounts, meters, active reservations,
// buyer identity, buyer contact info) for one subdivision (identified by
// T_Billing_Account.Project_ID) and creates the corresponding AquaScale-native
// records: one Subdivision (found-or-created by Name), one Property + Meter per
// WEBS billing account, and one Profile per distinct active buyer found via the
// reservation chain. T_PM_Reservation.Buyer_ID is the binding owner —
// co-buyers/family members in M_Buyer/M_Buyer_Contact who are NOT the bound
// reservation owner are never seeded.
//
// No local FK links Property directly to Profile — WEBS is the source of truth
// for that relationship. It's always derivable via:
//   Property.MirrorAccountNo -> T_Billing_Account.AccountNo -> ReservationNo
//   -> T_PM_Reservation.Buyer_ID -> Profile.BuyerRef
//
// IDEMPOTENCY: keyed on Property.MirrorAccountNo, not a stored project code —
// if any WEBS account for this Project_ID already has a Property row, this
// endpoint refuses to run (409) rather than risk partial duplicates.
//
// Dev-only tool, not a production feature.
//
// USAGE: POST /dev/seed-subdivision?projectId=TR01A&subdivisionName=Treelane%20Residences
// ============================================================================

app.MapPost("/dev/seed-subdivision", async (
    string projectId,
    string subdivisionName,
    AquaScale.Api.Data.AquaScaleDbContext aquaScaleDb,
    AquaScale.Api.Data.WebsDbContext websDb,
    IPasswordHasher<Profile> hasher,
    IQrCodeService qrCodeService) =>
{
    var targetAccounts = await websDb.BillingAccounts
        .Where(ba => ba.Project_ID == projectId)
        .ToListAsync();

    if (!targetAccounts.Any())
        return Results.NotFound($"No accounts found for Project_ID: {projectId}");

    var accountNos = targetAccounts.Select(ba => ba.AccountNo).ToList();
    var alreadySeeded = await aquaScaleDb.Properties
        .AnyAsync(p => p.MirrorAccountNo != null && accountNos.Contains(p.MirrorAccountNo));
    if (alreadySeeded)
        return Results.Conflict($"Some or all accounts for Project_ID '{projectId}' are already seeded. Delete the related Properties manually first if you want to re-seed.");

    var buyerRole = await aquaScaleDb.Roles.FirstOrDefaultAsync(r => r.Name == "Buyer");
    if (buyerRole is null)
        return Results.Problem("No 'Buyer' role exists yet — run /dev/seed-admin's role setup first, or seed the Buyer role manually.");

    // ── Batch prefetch #1: active reservations for every account in this batch ──
    // Confirmed one-buyer-per-ReservationNo (no COUNT(*) > 1 among active,
    // non-backed-out reservations), so a dictionary keyed by ReservationNo is safe.
    var reservationNos = targetAccounts
        .Where(ba => !string.IsNullOrWhiteSpace(ba.ReservationNo))
        .Select(ba => ba.ReservationNo!)
        .Distinct()
        .ToList();

    var reservationsByNo = await websDb.Reservations
        .Where(r => reservationNos.Contains(r.ReservationNo) && r.BackoutType == null)
        .ToDictionaryAsync(r => r.ReservationNo);

    // ── Batch prefetch #2: M_Buyer (names) for every bound owner in this batch ──
    var buyerIds = reservationsByNo.Values
        .Select(r => r.BuyerId.Trim())
        .Distinct()
        .ToList();
    
    var buyersById = await websDb.Buyers
        .Where(b => buyerIds.Contains(b.BuyerId))
        // FIX: Add .Trim() to the dictionary key
        .ToDictionaryAsync(b => b.BuyerId.Trim()); 

    // ── Batch prefetch #3: M_Buyer_Contact (email/mobile) for every bound owner ──
    var rawContacts = await websDb.BuyerContacts
        .Where(c => buyerIds.Contains(c.BuyerId))
        .OrderByDescending(c => c.DateUpdated)
        .ToListAsync();

    var contactByBuyerId = rawContacts
        // FIX: Add .Trim() to the grouping key to strip SQL char(8) padding
        .GroupBy(c => c.BuyerId.Trim()) 
        .ToDictionary(
            g => g.Key,
            g => g.FirstOrDefault(c => CleanContactValue(c.Email) is not null
                                    || CleanContactValue(c.MobileNo) is not null
                                    || CleanContactValue(c.TelNo) is not null));

    using var transaction = await aquaScaleDb.Database.BeginTransactionAsync();
    try
    {
        var subdivision = await aquaScaleDb.Subdivisions
            .FirstOrDefaultAsync(s => s.Name == subdivisionName);

        if (subdivision is null)
        {
            subdivision = new Subdivision
            {
                Id = Guid.NewGuid(),
                Name = subdivisionName,
                IsActive = true,
            };
            aquaScaleDb.Subdivisions.Add(subdivision);
        }

        var seededBuyerRefs = new HashSet<string>(
            await aquaScaleDb.Profiles
                .Where(p => p.BuyerRef != null)
                .Select(p => p.BuyerRef!)
                .ToListAsync());

        var buyersWithNoContact = new List<string>();

        foreach (var ba in targetAccounts)
        {
            var meter = await websDb.AccountMeters
                .FirstOrDefaultAsync(m => m.AccountNo == ba.AccountNo);
            if (meter is null) continue;

            string? compPbl = null;
            AquaScale.Api.Models.Webs.WEBSReservation? activeRes = null;
            if (!string.IsNullOrWhiteSpace(ba.ReservationNo)
                && reservationsByNo.TryGetValue(ba.ReservationNo, out var res))
            {
                activeRes = res;
                compPbl = activeRes?.CompPBL;
            }

            var propertyId = Guid.NewGuid();
            aquaScaleDb.Properties.Add(new Property
            {
                Id = propertyId,
                SubdivisionId = subdivision.Id,
                Block = "N/A", // TODO: parse from CompPbl once the real Block/Lot split rule is confirmed
                Lot = "N/A",   // TODO: same
                CompPbl = compPbl,
                MirrorAccountNo = ba.AccountNo,
                CreatedAt = DateTime.UtcNow,
            });

            var meterId = Guid.NewGuid();
            aquaScaleDb.Meters.Add(new Meter
            {
                Id = meterId,
                PropertyId = propertyId,
                MirrorAcctmtrId = meter.Id,
                UtilityType = "Water",
                QrCode = meterId.ToString(),
                CreatedAt = DateTime.UtcNow,
            });

            if (activeRes?.BuyerId is not null)
            {
                var buyerRef = activeRes.BuyerId.Trim();

                if (!seededBuyerRefs.Contains(buyerRef))
                {
                    buyersById.TryGetValue(buyerRef, out var websBuyer);
                    contactByBuyerId.TryGetValue(buyerRef, out var contact);

                    var email = CleanContactValue(contact?.Email);
                    var mobile = CleanContactValue(contact?.MobileNo);
                    var tel = CleanContactValue(contact?.TelNo);

                    if (email is null && mobile is null && tel is null)
                        buyersWithNoContact.Add(buyerRef);

                    var newProfile = new Profile
                    {
                        Id = Guid.NewGuid(),
                        RoleId = buyerRole.Id,
                        BuyerRef = buyerRef,
                        FullName = websBuyer?.BuyerName?.Trim() ?? buyerRef,
                        Email = email,
                        ContactNo = mobile ?? tel, // mobile preferred for SMS-based credential dissemination
                        IsActive = true,
                        MustChangePassword = true,
                        CreatedAt = DateTime.UtcNow,
                    };
                    newProfile.PasswordHash = hasher.HashPassword(newProfile, Guid.NewGuid().ToString());

                    aquaScaleDb.Profiles.Add(newProfile);
                    seededBuyerRefs.Add(buyerRef);
                }
            }
        }

        await aquaScaleDb.SaveChangesAsync();
        await transaction.CommitAsync();

        return Results.Ok(new
        {
            message = $"Successfully seeded '{subdivisionName}' ({projectId}).",
            accountsProcessed = targetAccounts.Count,
            buyersSeeded = seededBuyerRefs.Count,
            buyersMissingContactInfo = buyersWithNoContact.Count,
            buyerRefsMissingContact = buyersWithNoContact, // for manual follow-up before credential dissemination
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Results.Problem($"Seeding failed: {ex.Message}");
    }
});

// Normalizes a raw WEBS contact-field value: treats known placeholder tokens
// as absent, trims whitespace, and takes the first segment of "/"-separated
// dual-contact values (co-buyer contacts jammed into one field).
static string? CleanContactValue(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw)) return null;

    var value = raw.Split('/')[0].Trim();

    if (value is "N/A" or "N/I" or "-" or "." or "")
        return null;

    return value;
}
    
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
