using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.M_Buyer_Contact — buyer contact information.
/// Composite PK: Buyer_ID + DateUpdated, configured in WebsDbContext.
/// Multiple rows per buyer (update history) — always ORDER BY DateUpdated DESC.
/// Schema validated against WEBS-Schema.txt.
/// </summary>
public class WEBSBuyerContact
{
    [MaxLength(8)]
    public string   BuyerId     { get; set; } = null!; // Buyer_ID char(8) NOT NULL — composite PK, FK to M_Buyer
    public DateTime DateUpdated { get; set; }           // smalldatetime NOT NULL — composite PK
    [MaxLength(100)]
    public string?  Address1    { get; set; }           // nvarchar(100) NULL
    [MaxLength(100)]
    public string?  Address2    { get; set; }           // nvarchar(100) NULL
    [MaxLength(50)]
    public string?  Municipality { get; set; }          // nvarchar(50) NULL
    [MaxLength(50)]
    public string?  City        { get; set; }           // nvarchar(50) NULL
    [MaxLength(6)]
    public string?  ZipCode     { get; set; }           // char(6) NULL
    [MaxLength(30)]
    public string?  TelNo       { get; set; }           // nvarchar(30) NULL
    [MaxLength(30)]
    public string?  MobileNo    { get; set; }           // nvarchar(30) NULL
    [MaxLength(50)]
    public string?  Email       { get; set; }           // nvarchar(50) NULL
    // Sketch (image) excluded — binary blob irrelevant to AquaScale
    [MaxLength(100)]
    public string?  FacebookAcct { get; set; }          // char(100) NULL
}
