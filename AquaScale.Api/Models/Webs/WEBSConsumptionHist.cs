using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.T_Consumption_Hist — consumption audit/history records.
/// No PK declared in WEBS schema (IsPrimaryKey=0 for all columns).
/// ID is configured as the EF Core key via HasKey() in WebsDbContext.
/// Identical structure to T_Consumption but adds RecType, RecordedBy, DateRecorded, RecordedRemarks.
/// Schema validated against WEBS-Schema.txt.
/// </summary>
public class WEBSConsumptionHist
{
    public Guid     Id          { get; set; }        // uniqueidentifier NOT NULL (logical key)
    public Guid     AcctMtrId   { get; set; }        // AcctMtr_ID uniqueidentifier NOT NULL
    public int      SeqNo       { get; set; }        // int NOT NULL
    public DateTime DateRead    { get; set; }        // datetime NOT NULL
    public DateTime DueDate     { get; set; }        // datetime NOT NULL
    public double   PrevRead    { get; set; }        // float NOT NULL
    public double   CurRead     { get; set; }        // float NOT NULL
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
    [MaxLength(1)]
    public string?  RecType     { get; set; }        // char(1) NULL — history record type
    [MaxLength(10)]
    public string?  RecordedBy  { get; set; }        // varchar(10) NULL
    public DateTime? DateRecorded    { get; set; }   // smalldatetime NULL
    [MaxLength(500)]
    public string?  RecordedRemarks { get; set; }    // varchar(500) NULL
}
