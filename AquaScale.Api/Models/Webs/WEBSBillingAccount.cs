using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.T_Billing_Account — billing account master.
/// PK: AccountNo (nchar 10). Schema validated against WEBS-Schema.txt.
/// Cross-DB FK: AquaScale.properties.mirror_account_no → this.AccountNo (string).
/// BillType: W = Water, E = Electric.
/// </summary>
public class WEBSBillingAccount
{
    [MaxLength(10)]
    public string     AccountNo   { get; set; } = null!; // nchar(10) NOT NULL PK
    public DateTime   DateReg     { get; set; }           // datetime NOT NULL
    [MaxLength(10)]
    public string     Entity_ID   { get; set; } = null!; // nvarchar(10) NOT NULL
    [MaxLength(50)]
    public string     AccountName { get; set; } = null!; // varchar(50) NOT NULL
    [MaxLength(10)]
    public string?    Project_ID  { get; set; }           // char(10) NULL
    [MaxLength(8)]
    public string?    ReservationNo { get; set; }         // nchar(8) NULL
    [MaxLength(2)]
    public string     BillType    { get; set; } = null!; // varchar(2) NOT NULL — W=Water, E=Electric
    [MaxLength(2)]
    public string     ClassID     { get; set; } = null!; // varchar(2) NOT NULL
    public int        MotherAcct  { get; set; }           // int NOT NULL
    [MaxLength(10)]
    public string?    Reference   { get; set; }           // nchar(10) NULL
    public DateTime?  Movein      { get; set; }           // datetime NULL
    [MaxLength(2)]
    public string     AcctStatus  { get; set; } = null!; // nchar(2) NOT NULL
    [MaxLength(10)]
    public string     Createdby   { get; set; } = null!; // nvarchar(10) NOT NULL
    public DateTime   DateCreated { get; set; }           // smalldatetime NOT NULL
    [MaxLength(10)]
    public string?    Editedby    { get; set; }           // nvarchar(10) NULL
    public DateTime?  DateEdited  { get; set; }           // smalldatetime NULL
    public int?       ActualConsumptionID { get; set; }   // int NULL
}
