using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.T_Consumption — posted meter reading records.
/// PK: ID (uniqueidentifier). Schema validated against WEBS-Schema.txt.
/// AcctMtrId (AcctMtr_ID) is a FK to T_Account_Meter.ID.
/// All core billing columns are NOT NULL in WEBS.
/// </summary>
public class WEBSConsumption
{
    public Guid     Id          { get; set; }        // uniqueidentifier NOT NULL PK
    public Guid     AcctMtrId   { get; set; }        // AcctMtr_ID uniqueidentifier NOT NULL
    public int      SeqNo       { get; set; }        // int NOT NULL
    public DateTime DateRead    { get; set; }        // datetime NOT NULL
    public DateTime DueDate     { get; set; }        // datetime NOT NULL
    public double   PrevRead    { get; set; }        // float NOT NULL — confirmed NOT NULL
    public double   CurRead     { get; set; }        // float NOT NULL — confirmed NOT NULL
    public double   UsedPerMtr  { get; set; }        // float NOT NULL
    public decimal  RatePerMtr  { get; set; }        // money NOT NULL
    public decimal  CurCharge   { get; set; }        // money NOT NULL
    [MaxLength(500)]
    public string?  Remarks     { get; set; }        // nvarchar(500) NULL
    [MaxLength(10)]
    public string?  GlRefNo     { get; set; }        // GLRefNo nchar(10) NULL
    public bool?    IsEndBalance { get; set; }       // bit NULL
    [MaxLength(10)]
    public string?  CreatedBy   { get; set; }        // varchar(10) NULL
    public DateTime? DateCreated { get; set; }       // smalldatetime NULL
    [MaxLength(10)]
    public string?  UpdatedBy   { get; set; }        // varchar(10) NULL
    public DateTime? DateUpdated { get; set; }       // smalldatetime NULL
    [MaxLength(10)]
    public string?  RefNo       { get; set; }        // varchar(10) NULL
}
