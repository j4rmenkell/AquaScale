using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.M_Buyer — buyer personal information master.
/// Logical PK: Buyer_ID (char 8), configured as key in WebsDbContext with ValueGeneratedNever().
/// Cross-DB FK: AquaScale.profiles.buyer_ref → this.BuyerId (string).
/// M_Buyer has ~80 columns — only those needed by AquaScale are mapped here.
/// BuyerName is nullable — callers must fall back to LastName + ", " + FirstName.
/// Schema validated against WEBS-Schema.txt.
/// </summary>
public class WEBSBuyer
{
    [MaxLength(8)]
    public string  BuyerId   { get; set; } = null!; // Buyer_ID char(8) logical PK
    [MaxLength(100)]
    public string  LastName  { get; set; } = null!; // nvarchar(100) NOT NULL
    [MaxLength(100)]
    public string  FirstName { get; set; } = null!; // nvarchar(100) NOT NULL
    [MaxLength(100)]
    public string? MidName   { get; set; }           // nvarchar(100) NULL
    [MaxLength(200)]
    public string? BuyerName { get; set; }           // nvarchar(200) NULL — may be null, use Last+First
    [MaxLength(2)]
    public string? Status_ID { get; set; }           // char(1) NULL — buyer status
    public DateTime? DateUpdated { get; set; }       // smalldatetime NULL
}
