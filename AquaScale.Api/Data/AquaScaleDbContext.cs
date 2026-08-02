using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Models.Mirror;

namespace AquaScale.Api.Data;

public class AquaScaleDbContext : DbContext
{
    public AquaScaleDbContext(DbContextOptions<AquaScaleDbContext> options)
        : base(options)
    {
    }

    public DbSet<BillingStatement> BillingStatements { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Subdivision> Subdivisions { get; set; } = null!;
    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<Meter> Meters { get; set; } = null!;
    public DbSet<MeterReading> MeterReadings { get; set; } = null!;
    public DbSet<MirrorReservation> MirrorReservations { get; set; } = null!;
    public DbSet<MirrorBuyer> MirrorBuyers { get; set; } = null!;
    public DbSet<MirrorBuyerContact> MirrorBuyerContacts { get; set; } = null!;
    public DbSet<MirrorConsumption> MirrorConsumptions { get; set; } = null!;

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- billing_statements: scaffolded from existing Supabase table ---
        modelBuilder.Entity<BillingStatement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("billing_statements_pkey");
            entity.ToTable("billing_statements");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.CurrentCharge).HasColumnName("current_charge");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.MeterReadingId).HasColumnName("meter_reading_id");
            entity.Property(e => e.Penalty).HasDefaultValueSql("0").HasColumnName("penalty");
            entity.Property(e => e.PreviousBalance).HasDefaultValueSql("0").HasColumnName("previous_balance");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.Status).HasDefaultValueSql("'Draft'::text").HasColumnName("status");
            entity.Property(e => e.SupersededBy).HasColumnName("superseded_by");
            entity.Property(e => e.TotalAmountDue).HasColumnName("total_amount_due");
            entity.Property(e => e.UtilityType).HasColumnName("utility_type");
            entity.Property(e => e.VoidedAt).HasColumnName("voided_at");
            entity.Property(e => e.WebsConsumptionId).HasColumnName("webs_consumption_id");
            entity.Property(e => e.WebsSeqNo).HasColumnName("webs_seq_no");
            entity.Property(e => e.WebsSyncError).HasColumnName("webs_sync_error");
            entity.Property(e => e.WebsSyncStatus).HasDefaultValueSql("'Pending'::text").HasColumnName("webs_sync_status");
        });

        // --- role_permissions: composite key, matches ERD's PK-FK/PK-FK notation ---
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

        // --- profiles: FK to roles ---
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasOne(p => p.Role)
                .WithMany(r => r.Profiles)
                .HasForeignKey(p => p.RoleId);
        });

        modelBuilder.Entity<MirrorAccountMeter>(entity =>
        {
            entity.ToTable("mirror_account_meters", t => t.ExcludeFromMigrations());
            // Syncer owns writes. Never SaveChanges() against this entity from Api.
        });

        modelBuilder.Entity<Meter>(entity =>
        {
            entity.HasOne(m => m.MirrorAccountMeter)
                .WithMany()
                .HasForeignKey(m => m.MirrorAcctmtrId)
                .OnDelete(DeleteBehavior.NoAction); // don't cascade into a Syncer-owned table
        });

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
        modelBuilder.Entity<MirrorReservation>(entity =>
        {
            entity.ToTable("mirror_reservations", t => t.ExcludeFromMigrations());
        });

        modelBuilder.Entity<MirrorBuyer>(entity =>
        {
            entity.ToTable("mirror_buyers", t => t.ExcludeFromMigrations());
        });
        modelBuilder.Entity<MirrorBuyerContact>(entity =>
        {
            entity.ToTable("mirror_buyer_contacts", t => t.ExcludeFromMigrations());
        });
        modelBuilder.Entity<MirrorConsumption>(entity =>
        {
            entity.ToTable("mirror_consumptions", t => t.ExcludeFromMigrations());
            
            entity.Property(e => e.CurRead)
                .HasColumnType("double precision")
                .HasConversion<double>(); // <-- This is required to stop the crash
                
            entity.Property(e => e.PrevRead)
                .HasColumnType("double precision")
                .HasConversion<double>(); // <-- This is required to stop the crash
        });
    }
}