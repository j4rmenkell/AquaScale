using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.M_SystemParam — global billing configuration parameters.
/// No PK declared in WEBS schema (all IsPrimaryKey=0). This is a single-row config table.
/// Configured with HasNoKey() in WebsDbContext — use AsNoTracking() always.
/// Schema validated against WEBS-Schema.txt. Only billing-relevant columns mapped.
/// </summary>
public class WEBSSystemParam
{
    [MaxLength(100)]
    public string?  Comp_Name              { get; set; } // nvarchar(100) NULL
    [MaxLength(100)]
    public string?  Address                { get; set; } // nvarchar(100) NULL
    public decimal? WaterNoUsedCharge      { get; set; } // money NULL — min charge when no consumption
    public decimal? ElectNoUsedCharge      { get; set; } // money NULL
    public decimal? WaterGracePeriod       { get; set; } // numeric(18,0) NULL — days before penalty
    public decimal? ElectGracePeriod       { get; set; } // numeric(18,0) NULL
    public decimal? WaterPenaltyRate       { get; set; } // money NULL
    public decimal? ElectPenaltyRate       { get; set; } // money NULL
    public decimal? NoDaysToDue            { get; set; } // numeric(18,0) NULL
    [MaxLength(50)]
    public string?  WaterReconChargeID     { get; set; } // nvarchar(50) NULL
    [MaxLength(10)]
    public string?  ElectricReconChargeID  { get; set; } // nvarchar(10) NULL
    public decimal? WaterNoMoDelay         { get; set; } // numeric(18,0) NULL
    public decimal? ElectricNoMoDelay      { get; set; } // numeric(18,0) NULL
    public decimal? WaterDisconnDays       { get; set; } // numeric(18,0) NULL
    public decimal? WaterDisconnAmt        { get; set; } // money NULL
    public decimal  PaymentServer          { get; set; } // numeric(18,0) NOT NULL
    [MaxLength(10)]
    public string?  CompID                 { get; set; } // nchar(10) NULL
    [MaxLength(10)]
    public string?  Editedby               { get; set; } // nvarchar(10) NULL
    public DateTime? DateEdited            { get; set; } // smalldatetime NULL
}
