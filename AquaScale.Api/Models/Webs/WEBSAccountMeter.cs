using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.T_Account_Meter — physical meter master.
/// PK: ID (uniqueidentifier). Schema validated against WEBS-Schema.txt.
/// Cross-DB FK: AquaScale.meters.mirror_acctmtr_id → this.Id (Guid).
/// WithIssue and Hold are confirmed real columns per WEBS-Schema.txt.
/// </summary>
public class WEBSAccountMeter
{
    public Guid    Id          { get; set; }        // uniqueidentifier NOT NULL PK
    [MaxLength(10)]
    public string  AccountNo   { get; set; } = null!; // nchar(10) NOT NULL
    [MaxLength(15)]
    public string  MeterNo     { get; set; } = null!; // nvarchar(15) NOT NULL
    public DateTime?  DateInstalled   { get; set; } // datetime NULL
    public decimal?   DepAmt          { get; set; } // money NULL
    public DateTime?  DepDate         { get; set; } // datetime NULL
    [MaxLength(2)]
    public string?    MeterStatus     { get; set; } // nchar(2) NULL — M_GenCodes Group 41
    public DateTime?  StatusDate      { get; set; } // datetime NULL
    [MaxLength(500)]
    public string?    Remarks         { get; set; } // nvarchar(500) NULL
    [MaxLength(100)]
    public string?    Notes           { get; set; } // nvarchar(100) NULL
    [MaxLength(10)]
    public string     Createdby       { get; set; } = null!; // nvarchar(10) NOT NULL
    public DateTime   DateCreated     { get; set; }           // smalldatetime NOT NULL
    [MaxLength(10)]
    public string?    Editedby        { get; set; }            // nvarchar(10) NULL
    public DateTime?  DateEdited      { get; set; }            // smalldatetime NULL
    public int?       ActualConsumptionID { get; set; }        // int NULL
    public bool?      WithIssue       { get; set; }            // bit NULL — confirmed in WEBS-Schema.txt
    public bool?      Hold            { get; set; }            // bit NULL — confirmed in WEBS-Schema.txt
}
