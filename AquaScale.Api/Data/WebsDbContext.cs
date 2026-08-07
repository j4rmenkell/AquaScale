using Microsoft.EntityFrameworkCore;
using AquaScale.Api.Models.Webs;

namespace AquaScale.Api.Data;

/// <summary>
/// DbContext for the WEBS database (legacy Charles Builders billing system).
///
/// RULES — read before touching this file:
///   1. NEVER run EF Core migrations against this context.
///      WEBS owns its own schema. Always use --context AquaScaleDbContext for migrations.
///   2. WEBS does NOT use SQL IDENTITY for any transaction keys. M_ControlNo is the
///      manual sequence generator. All PKs use ValueGeneratedNever().
///   3. All 12 registered entities use ExcludeFromMigrations() so EF Core will
///      never generate DDL for them.
///   4. For pure reads, always use AsNoTracking() to avoid unnecessary tracking overhead.
///   5. For writes to T_Consumption / T_Payment, use tracked entities and call
///      SaveChangesAsync() on this context. Coordinate with AquaScaleDbContext writes
///      in a try/catch — WEBS is the source of truth.
/// </summary>
public class WebsDbContext : DbContext
{
    public WebsDbContext(DbContextOptions<WebsDbContext> options)
        : base(options)
    {
    }

    // ── The 12 required WEBS tables ─────────────────────────────────────────
    public DbSet<WEBSAccountMeter>    AccountMeters    { get; set; } = null!;  // T_Account_Meter
    public DbSet<WEBSBillingAccount>  BillingAccounts  { get; set; } = null!;  // T_Billing_Account
    public DbSet<WEBSConsumption>     Consumptions     { get; set; } = null!;  // T_Consumption
    public DbSet<WEBSConsumptionHist> ConsumptionHists { get; set; } = null!;  // T_Consumption_Hist
    public DbSet<WEBSPayment>         Payments         { get; set; } = null!;  // T_Payment
    public DbSet<WEBSPaymentHist>     PaymentHists     { get; set; } = null!;  // T_Payment_Hist
    public DbSet<WEBSReservation>     Reservations     { get; set; } = null!;  // T_PM_Reservation
    public DbSet<WEBSBuyer>           Buyers           { get; set; } = null!;  // M_Buyer
    public DbSet<WEBSBuyerContact>    BuyerContacts    { get; set; } = null!;  // M_Buyer_Contact
    public DbSet<WEBSGenCodes>        GenCodes         { get; set; } = null!;  // M_GenCodes
    public DbSet<WEBSSystemParam>     SystemParams     { get; set; } = null!;  // M_SystemParam (HasNoKey)
    public DbSet<WEBSControlNo>       ControlNos       { get; set; } = null!;  // M_ControlNo

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── T_Account_Meter ──────────────────────────────────────────────────
        // PK: ID (uniqueidentifier). Cross-DB FK: AquaScale.meters.mirror_acctmtr_id → ID (Guid).
        // Schema: WEBS-Schema.txt confirmed. WithIssue + Hold are real columns.
        modelBuilder.Entity<WEBSAccountMeter>(e =>
        {
            e.ToTable("T_Account_Meter", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
            e.Property(x => x.AccountNo).HasColumnType("nchar(10)");
            e.Property(x => x.MeterNo).HasColumnType("nvarchar(15)");
            e.Property(x => x.DepAmt).HasColumnType("money");
            e.Property(x => x.MeterStatus).HasColumnType("nchar(2)");
            e.Property(x => x.Remarks).HasColumnType("nvarchar(500)");
            e.Property(x => x.Notes).HasColumnType("nvarchar(100)");
            e.Property(x => x.Createdby).HasColumnType("nvarchar(10)");
            e.Property(x => x.Editedby).HasColumnType("nvarchar(10)");
        });

        // ── T_Billing_Account ────────────────────────────────────────────────
        // PK: AccountNo (nchar 10). Cross-DB FK: AquaScale.properties.mirror_account_no → AccountNo (string).
        modelBuilder.Entity<WEBSBillingAccount>(e =>
        {
            e.ToTable("T_Billing_Account", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.AccountNo);
            e.Property(x => x.AccountNo).HasColumnType("nchar(10)").ValueGeneratedNever();
            e.Property(x => x.Entity_ID).HasColumnType("nvarchar(10)");
            e.Property(x => x.AccountName).HasColumnType("varchar(50)");
            e.Property(x => x.Project_ID).HasColumnType("char(10)");
            e.Property(x => x.ReservationNo).HasColumnType("nchar(8)");
            e.Property(x => x.BillType).HasColumnType("varchar(2)");
            e.Property(x => x.ClassID).HasColumnType("varchar(2)");
            e.Property(x => x.Reference).HasColumnType("nchar(10)");
            e.Property(x => x.AcctStatus).HasColumnType("nchar(2)");
            e.Property(x => x.Createdby).HasColumnType("nvarchar(10)");
            e.Property(x => x.Editedby).HasColumnType("nvarchar(10)");
        });

        // ── T_Consumption ────────────────────────────────────────────────────
        // PK: ID (uniqueidentifier). AcctMtr_ID → underscore column name.
        modelBuilder.Entity<WEBSConsumption>(e =>
        {
            e.ToTable("T_Consumption", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
            e.Property(x => x.AcctMtrId).HasColumnName("AcctMtr_ID");
            e.Property(x => x.PrevRead).HasColumnType("float");
            e.Property(x => x.CurRead).HasColumnType("float");
            e.Property(x => x.UsedPerMtr).HasColumnType("float");
            e.Property(x => x.RatePerMtr).HasColumnType("money");
            e.Property(x => x.CurCharge).HasColumnType("money");
            e.Property(x => x.Remarks).HasColumnType("nvarchar(500)");
            e.Property(x => x.GlRefNo).HasColumnName("GLRefNo").HasColumnType("nchar(10)");
            e.Property(x => x.CreatedBy).HasColumnType("varchar(10)");
            e.Property(x => x.UpdatedBy).HasColumnType("varchar(10)");
            e.Property(x => x.RefNo).HasColumnType("varchar(10)");
        });

        // ── T_Consumption_Hist ───────────────────────────────────────────────
        // No PK in WEBS schema — ID configured as logical key with ValueGeneratedNever().
        modelBuilder.Entity<WEBSConsumptionHist>(e =>
        {
            e.ToTable("T_Consumption_Hist", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
            e.Property(x => x.AcctMtrId).HasColumnName("AcctMtr_ID");
            e.Property(x => x.PrevRead).HasColumnType("float");
            e.Property(x => x.CurRead).HasColumnType("float");
            e.Property(x => x.UsedPerMtr).HasColumnType("float");
            e.Property(x => x.RatePerMtr).HasColumnType("money");
            e.Property(x => x.CurCharge).HasColumnType("money");
            e.Property(x => x.Remarks).HasColumnType("nvarchar(500)");
            e.Property(x => x.GlRefNo).HasColumnName("GLRefNo").HasColumnType("nchar(10)");
            e.Property(x => x.CreatedBy).HasColumnType("varchar(10)");
            e.Property(x => x.UpdatedBy).HasColumnType("varchar(10)");
            e.Property(x => x.RefNo).HasColumnType("varchar(10)");
            e.Property(x => x.RecType).HasColumnType("char(1)");
            e.Property(x => x.RecordedBy).HasColumnType("varchar(10)");
            e.Property(x => x.RecordedRemarks).HasColumnType("varchar(500)");
        });

        // ── T_Payment ────────────────────────────────────────────────────────
        // No declared PK in WEBS schema — ID configured as logical key.
        // ORDate/DepoDate/DueDate are SQL Server 'date' type → DateOnly? in C#.
        // AcctMtr_ID and ConsumptionID use underscore column names.
        modelBuilder.Entity<WEBSPayment>(e =>
        {
            e.ToTable("T_Payment", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
            e.Property(x => x.AcctMtrId).HasColumnName("AcctMtr_ID");
            e.Property(x => x.OrDate).HasColumnName("ORDate").HasColumnType("date");
            e.Property(x => x.DepoDate).HasColumnType("date");
            e.Property(x => x.BankDepo).HasColumnType("nvarchar(10)");
            e.Property(x => x.BankAcctNo).HasColumnType("varchar(20)");
            e.Property(x => x.DueDate).HasColumnType("date");
            e.Property(x => x.OrNo).HasColumnName("ORNo").HasColumnType("nvarchar(10)");
            e.Property(x => x.CurCharge).HasColumnType("money");
            e.Property(x => x.Penalty).HasColumnType("money");
            e.Property(x => x.TotAmtDue).HasColumnType("money");
            e.Property(x => x.AmtPaid).HasColumnType("money");
            e.Property(x => x.Balance).HasColumnType("money");
            e.Property(x => x.PrevBalance).HasColumnType("money");
            e.Property(x => x.PrevPenalty).HasColumnType("money");
            e.Property(x => x.WaivePenalty).HasColumnType("nvarchar(2)");
            e.Property(x => x.Remarks).HasColumnType("nvarchar(500)");
            e.Property(x => x.ConsumptionId).HasColumnName("ConsumptionID");
            e.Property(x => x.GlRefNo).HasColumnName("GLRefNo").HasColumnType("nchar(10)");
            e.Property(x => x.CreatedBy).HasColumnType("varchar(10)");
            e.Property(x => x.EditedBy).HasColumnType("varchar(10)");
            e.Property(x => x.Approved).HasColumnType("varchar(10)");
            e.Property(x => x.DeletedBy).HasColumnType("varchar(10)");
            e.Property(x => x.ParticularId).HasColumnName("ParticularID").HasColumnType("char(2)");
            e.Property(x => x.CancelledBy).HasColumnType("varchar(10)");
            e.Property(x => x.CancelRemarks).HasColumnType("varchar(255)");
            e.Property(x => x.PrintedOr).HasColumnName("PrintedOR");
            e.Property(x => x.RefNo).HasColumnType("varchar(20)");
        });

        // ── T_Payment_Hist ───────────────────────────────────────────────────
        // No declared PK — composite logical key: ID + DeleteCounter.
        // ORDate/DepoDate/DueDate are datetime (NOT date, unlike T_Payment).
        modelBuilder.Entity<WEBSPaymentHist>(e =>
        {
            e.ToTable("T_Payment_Hist", t => t.ExcludeFromMigrations());
            e.HasKey(x => new { x.Id, x.DeleteCounter });
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
            e.Property(x => x.DeleteCounter).ValueGeneratedNever();
            e.Property(x => x.AcctMtrId).HasColumnName("AcctMtr_ID");
            e.Property(x => x.OrDate).HasColumnName("ORDate");
            e.Property(x => x.BankDepo).HasColumnType("nvarchar(10)");
            e.Property(x => x.BankAcctNo).HasColumnType("nchar(20)");
            e.Property(x => x.OrNo).HasColumnName("ORNo").HasColumnType("nvarchar(10)");
            e.Property(x => x.CurCharge).HasColumnType("money");
            e.Property(x => x.Penalty).HasColumnType("money");
            e.Property(x => x.TotAmtDue).HasColumnType("money");
            e.Property(x => x.AmtPaid).HasColumnType("money");
            e.Property(x => x.Balance).HasColumnType("money");
            e.Property(x => x.PrevBalance).HasColumnType("money");
            e.Property(x => x.PrevPenalty).HasColumnType("money");
            e.Property(x => x.WaivePenalty).HasColumnType("nvarchar(2)");
            e.Property(x => x.Remarks).HasColumnType("nvarchar(500)");
            e.Property(x => x.ConsumptionId).HasColumnName("ConsumptionID");
            e.Property(x => x.GlRefNo).HasColumnName("GLRefNo").HasColumnType("nchar(10)");
            e.Property(x => x.RecType).HasColumnType("nchar(1)");
            e.Property(x => x.UpdatedBy).HasColumnType("nchar(10)");
            e.Property(x => x.PrintedOr).HasColumnName("PrintedOR");
            e.Property(x => x.RefNo).HasColumnType("varchar(20)");
        });

        // ── T_PM_Reservation ─────────────────────────────────────────────────
        // Logical PK: ReservationNo (char 8). BackoutType IS NULL = active reservation.
        // Cross-DB use: CompPBL matches Property.CompPbl in AquaScale.
        modelBuilder.Entity<WEBSReservation>(e =>
        {
            e.ToTable("T_PM_Reservation", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.ReservationNo);
            e.Property(x => x.ReservationNo).HasColumnType("char(8)").ValueGeneratedNever();
            e.Property(x => x.CompPBL).HasColumnType("varchar(40)");
            e.Property(x => x.Scheme_ID).HasColumnType("char(2)");
            e.Property(x => x.Bank_ID).HasColumnType("char(10)");
            e.Property(x => x.BuyerId).HasColumnName("Buyer_ID").HasColumnType("char(8)");
            e.Property(x => x.Broker_ID).HasColumnType("varchar(10)");
            e.Property(x => x.Agent_ID).HasColumnType("varchar(10)");
            e.Property(x => x.BackoutType).HasColumnType("char(2)");
            e.Property(x => x.Status).HasColumnType("char(2)");
        });

        // ── M_Buyer ──────────────────────────────────────────────────────────
        // Logical PK: Buyer_ID (char 8). Cross-DB FK: AquaScale.profiles.buyer_ref → BuyerId.
        // BuyerName is nullable — callers must fall back to LastName+FirstName.
        modelBuilder.Entity<WEBSBuyer>(e =>
        {
            e.ToTable("M_Buyer", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.BuyerId);
            e.Property(x => x.BuyerId).HasColumnName("Buyer_ID").HasColumnType("char(8)").ValueGeneratedNever();
            e.Property(x => x.LastName).HasColumnType("nvarchar(100)");
            e.Property(x => x.FirstName).HasColumnType("nvarchar(100)");
            e.Property(x => x.MidName).HasColumnType("nvarchar(100)");
            e.Property(x => x.BuyerName).HasColumnType("nvarchar(200)");
            e.Property(x => x.Status_ID).HasColumnType("char(1)");
        });

        // ── M_Buyer_Contact ──────────────────────────────────────────────────
        // Composite PK: Buyer_ID + DateUpdated. Multiple rows per buyer (history).
        // Always ORDER BY DateUpdated DESC, take first non-null for each contact field.
        modelBuilder.Entity<WEBSBuyerContact>(e =>
        {
            e.ToTable("M_Buyer_Contact", t => t.ExcludeFromMigrations());
            e.HasKey(x => new { x.BuyerId, x.DateUpdated });
            e.Property(x => x.BuyerId).HasColumnName("Buyer_ID").HasColumnType("char(8)").ValueGeneratedNever();
            e.Property(x => x.Address1).HasColumnType("nvarchar(100)");
            e.Property(x => x.Address2).HasColumnType("nvarchar(100)");
            e.Property(x => x.Municipality).HasColumnType("nvarchar(50)");
            e.Property(x => x.City).HasColumnType("nvarchar(50)");
            e.Property(x => x.ZipCode).HasColumnType("char(6)");
            e.Property(x => x.TelNo).HasColumnType("nvarchar(30)");
            e.Property(x => x.MobileNo).HasColumnType("nvarchar(30)");
            e.Property(x => x.Email).HasColumnType("nvarchar(50)");
            e.Property(x => x.FacebookAcct).HasColumnName("FacebookAcct").HasColumnType("char(100)");
        });

        // ── M_GenCodes ───────────────────────────────────────────────────────
        // Composite PK: Group_Code + Field_Code (both char 2).
        // Used by AquaScale to decode meter status, BackoutType, AcctStatus codes.
        modelBuilder.Entity<WEBSGenCodes>(e =>
        {
            e.ToTable("M_GenCodes", t => t.ExcludeFromMigrations());
            e.HasKey(x => new { x.GroupCode, x.FieldCode });
            e.Property(x => x.GroupCode).HasColumnName("Group_Code").HasColumnType("char(2)").ValueGeneratedNever();
            e.Property(x => x.FieldCode).HasColumnName("Field_Code").HasColumnType("char(2)").ValueGeneratedNever();
            e.Property(x => x.Description).HasColumnType("nvarchar(50)");
            e.Property(x => x.Status_ID).HasColumnType("char(1)");
            e.Property(x => x.ShortDesc).HasColumnType("varchar(10)");
        });

        // ── M_SystemParam ────────────────────────────────────────────────────
        // Single-row config table. No PK in WEBS schema → HasNoKey().
        // ALWAYS use AsNoTracking() when querying. Never SaveChanges() against this.
        modelBuilder.Entity<WEBSSystemParam>(e =>
        {
            e.ToTable("M_SystemParam", t => t.ExcludeFromMigrations());
            e.HasNoKey();
            e.Property(x => x.Comp_Name).HasColumnType("nvarchar(100)");
            e.Property(x => x.Address).HasColumnType("nvarchar(100)");
            e.Property(x => x.WaterNoUsedCharge).HasColumnType("money");
            e.Property(x => x.ElectNoUsedCharge).HasColumnType("money");
            e.Property(x => x.WaterGracePeriod).HasColumnType("numeric(18,0)");
            e.Property(x => x.ElectGracePeriod).HasColumnType("numeric(18,0)");
            e.Property(x => x.WaterPenaltyRate).HasColumnType("money");
            e.Property(x => x.ElectPenaltyRate).HasColumnType("money");
            e.Property(x => x.NoDaysToDue).HasColumnType("numeric(18,0)");
            e.Property(x => x.WaterReconChargeID).HasColumnType("nvarchar(50)");
            e.Property(x => x.ElectricReconChargeID).HasColumnType("nvarchar(10)");
            e.Property(x => x.WaterNoMoDelay).HasColumnType("numeric(18,0)");
            e.Property(x => x.ElectricNoMoDelay).HasColumnType("numeric(18,0)");
            e.Property(x => x.WaterDisconnDays).HasColumnType("numeric(18,0)");
            e.Property(x => x.WaterDisconnAmt).HasColumnType("money");
            e.Property(x => x.PaymentServer).HasColumnType("numeric(18,0)");
            e.Property(x => x.CompID).HasColumnType("nchar(10)");
            e.Property(x => x.Editedby).HasColumnType("nvarchar(10)");
        });

        // ── M_ControlNo ──────────────────────────────────────────────────────
        // Manual sequence generator. WEBS does NOT use SQL IDENTITY.
        // All transaction keys (ORNo, etc.) are generated here via increment of last_series_no.
        // ValueGeneratedNever() on ControlId — WEBS fills this, not SQL Server.
        modelBuilder.Entity<WEBSControlNo>(e =>
        {
            e.ToTable("M_ControlNo", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.ControlId);
            e.Property(x => x.ControlId).HasColumnName("control_id").ValueGeneratedNever();
            e.Property(x => x.ControlType).HasColumnName("control_type").HasColumnType("varchar(15)");
            e.Property(x => x.ControlDesc).HasColumnName("control_desc").HasColumnType("varchar(50)");
            e.Property(x => x.ControlCoId).HasColumnName("control_co_id").HasColumnType("varchar(10)");
            e.Property(x => x.ControlMode).HasColumnName("control_mode").HasColumnType("varchar(1)");
            e.Property(x => x.ControlYear).HasColumnName("control_year").HasColumnType("varchar(2)");
            e.Property(x => x.ControlPeriod).HasColumnName("control_period").HasColumnType("varchar(2)");
            e.Property(x => x.DataWidth).HasColumnName("data_width");
            e.Property(x => x.LastSeriesNo).HasColumnName("last_series_no");
            e.Property(x => x.StatusId).HasColumnName("status_id").HasColumnType("varchar(1)");
        });
    }
}
