using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Data; // Update this if your context is in a different namespace
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Services;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using AquaScale.Api.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AquaScaleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseDb"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<PropertyOwnershipService>();

// CHANGED: session-cookie auth, per the decision to use ASP.NET's auth engine against the
// custom `profiles` table rather than default Identity schema / Supabase Auth. Uses
// PasswordHasher<Profile> directly (same hashing algorithm Identity uses internally)
// instead of full UserManager/SignInManager, since that would require custom IUserStore<T>
// plumbing for marginal benefit here.

builder.Services.AddSingleton<IPasswordHasher<Profile>, PasswordHasher<Profile>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "aquascale_session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None; // TODO: confirm against actual frontend origin —
                                                      // None is required for cross-site cookies (e.g.
                                                      // Capacitor app origin != API origin), but if
                                                      // frontend and API end up same-site, Lax is safer.
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // requires HTTPS — fine given DigitalOcean+nginx+SSL plan
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        // This is an API, not an MVC app with login pages — override the default
        // redirect-to-login-page behavior with plain status codes instead.
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



builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// TODO: CORS is required once the frontend runs on a different origin than the API —
// cookie auth will not work cross-origin without an explicit policy allowing credentials.
// Not added yet since the actual frontend origin(s) (dev + prod) aren't confirmed here.
// Example, once known:
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("Frontend", policy =>
//         policy.WithOrigins("https://your-frontend-origin.example")
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials());
// });

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Dev-only: creates one Admin role (if missing) and one test Profile with a real
    // PasswordHasher<Profile> hash, so /api/auth/login has something to authenticate
    // against. Gated behind IsDevelopment() — never reachable in production.
    app.MapPost("/dev/seed-admin", async (AquaScaleDbContext db, IPasswordHasher<Profile> hasher, IConfiguration config) =>
    {
        const string devEmail = "admin@aquascale.dev";
        // Pull from config/env instead of a literal string — still trivially easy for
        // local dev (one line in appsettings.Development.json, gitignored), but never
        // committed as plaintext.
        var devPassword = config["DevSeed:AdminPassword"] 
            ?? throw new InvalidOperationException("DevSeed:AdminPassword not configured.");

        if (await db.Profiles.AnyAsync(p => p.Email == devEmail))
        {
            return Results.Conflict("Dev admin already seeded.");
        }

        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null)
        {
            adminRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                IsSystem = true,
                Position = 0,
            };
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
            PasswordHash = string.Empty, // set below, needs the profile object to exist first
        };
        profile.PasswordHash = hasher.HashPassword(profile, devPassword);

        db.Profiles.Add(profile);
        await db.SaveChangesAsync();

        return Results.Ok(new { email = devEmail, password = devPassword });
    });
}

app.UseHttpsRedirection();

// app.UseCors("Frontend"); // uncomment once the CORS policy above is configured

// CHANGED: order matters — authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}