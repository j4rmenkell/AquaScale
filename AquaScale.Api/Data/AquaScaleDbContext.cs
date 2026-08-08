using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Models.AquaScale;

namespace AquaScale.Api.Data;

/// <summary>
/// DbContext for the AquaScale database.
/// Owns application-specific metadata: user accounts, roles, permissions,
/// subdivisions, properties, meters, meter readings (photo URLs, GPS, OCR),
/// and billing statement records.
///
/// EF Core migrations target THIS context only.
/// For WEBS billing/consumption data, use WebsDbContext.
/// </summary>
public class AquaScaleDbContext : DbContext
{
    public AquaScaleDbContext(DbContextOptions<AquaScaleDbContext> options)
        : base(options)
    {
    }

    // ── AquaScale-owned tables ───────────────────────────────────────────────
    public DbSet<BillingStatement> BillingStatements { get; set; } = null!;
    public DbSet<Role>             Roles             { get; set; } = null!;
    public DbSet<Permission>       Permissions       { get; set; } = null!;
    public DbSet<RolePermission>   RolePermissions   { get; set; } = null!;
    public DbSet<Profile>          Profiles          { get; set; } = null!;
    public DbSet<Subdivision>      Subdivisions      { get; set; } = null!;
    public DbSet<Property>         Properties        { get; set; } = null!;
    public DbSet<Meter>            Meters            { get; set; } = null!;
    public DbSet<MeterReading>     MeterReadings     { get; set; } = null!;
    public DbSet<ServiceRequest>   ServiceRequests   { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── BillingStatement ─────────────────────────────────────────────────
        modelBuilder.Entity<BillingStatement>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Status).HasDefaultValueSql("'Draft'");
            entity.Property(e => e.Penalty).HasDefaultValueSql("0");
            entity.Property(e => e.PreviousBalance).HasDefaultValueSql("0");
            entity.Property(e => e.WebsSyncStatus).HasDefaultValueSql("'Pending'");
        });

        // ── RolePermission (composite PK) ────────────────────────────────────
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        });

        // ── Profile ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasOne(p => p.Role)
                .WithMany(r => r.Profiles)
                .HasForeignKey(p => p.RoleId);
        });

        // ── MeterReading ──────────────────────────────────────────────────────
        // Two FK paths from MeterReading back to Profile require explicit
        // DeleteBehavior.Restrict to avoid multiple cascade paths (SQL Server rule).
        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.HasOne(mr => mr.FieldWorker)
                .WithMany()
                .HasForeignKey(mr => mr.FieldWorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(mr => mr.Reviewer)
                .WithMany()
                .HasForeignKey(mr => mr.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ServiceRequest ───────────────────────────────────────────────────
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Status).HasDefaultValueSql("'Open'");

            entity.HasOne(sr => sr.Property)
                .WithMany()
                .HasForeignKey(sr => sr.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sr => sr.Meter)
                .WithMany()
                .HasForeignKey(sr => sr.MeterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(sr => sr.Customer)
                .WithMany()
                .HasForeignKey(sr => sr.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sr => sr.AssignedToProfile)
                .WithMany()
                .HasForeignKey(sr => sr.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}