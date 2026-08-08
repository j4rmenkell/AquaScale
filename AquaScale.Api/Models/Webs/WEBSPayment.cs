using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.T_Payment — payment transaction history per account-meter.
/// No PK declared in WEBS schema (IsPrimaryKey=0 for all columns).
/// ID is configured as the EF Core key via HasKey() in WebsDbContext with ValueGeneratedNever().
/// IMPORTANT: ORDate, DepoDate, DueDate are SQL Server 'date' type → DateOnly? in C#.
/// Schema validated against WEBS-Schema.txt.
/// </summary>
public class WEBSPayment
{
    public Guid      Id          { get; set; }       // uniqueidentifier NOT NULL (logical key)
    public Guid      AcctMtrId   { get; set; }       // AcctMtr_ID uniqueidentifier NOT NULL
    public int       SeqNo       { get; set; }       // int NOT NULL
    public DateOnly? OrDate      { get; set; }       // ORDate date NULL — SQL 'date' type
    public DateOnly? DepoDate    { get; set; }       // DepoDate date NULL — SQL 'date' type
    [MaxLength(10)]
    public string?   BankDepo    { get; set; }       // nvarchar(10) NULL
    [MaxLength(20)]
    public string?   BankAcctNo  { get; set; }       // varchar(20) NULL
    public DateOnly? DueDate     { get; set; }       // date NULL — SQL 'date' type
    [MaxLength(10)]
    public string?   OrNo        { get; set; }       // ORNo nvarchar(10) NULL
    public decimal?  CurCharge   { get; set; }       // money NULL
    public decimal?  Penalty     { get; set; }       // money NULL
    public decimal?  TotAmtDue   { get; set; }       // money NULL
    public decimal?  AmtPaid     { get; set; }       // money NULL
    public decimal?  Balance     { get; set; }       // money NULL
    public decimal?  PrevBalance { get; set; }       // money NULL
    public decimal?  PrevPenalty { get; set; }       // money NULL
    [MaxLength(2)]
    public string?   WaivePenalty { get; set; }      // nvarchar(2) NULL
    [MaxLength(500)]
    public string?   Remarks     { get; set; }       // nvarchar(500) NULL
    public Guid?     ConsumptionId { get; set; }     // ConsumptionID uniqueidentifier NULL
    [MaxLength(10)]
    public string?   GlRefNo     { get; set; }       // GLRefNo nchar(10) NULL
    [MaxLength(10)]
    public string?   CreatedBy   { get; set; }       // varchar(10) NULL
    public DateTime? DateCreated { get; set; }       // smalldatetime NULL
    [MaxLength(10)]
    public string?   EditedBy    { get; set; }       // varchar(10) NULL
    public DateTime? DateEdited  { get; set; }       // smalldatetime NULL
    [MaxLength(10)]
    public string?   Approved    { get; set; }       // varchar(10) NULL
    [MaxLength(10)]
    public string?   DeletedBy   { get; set; }       // varchar(10) NULL
    [MaxLength(2)]
    public string?   ParticularId { get; set; }      // ParticularID char(2) NULL
    [MaxLength(10)]
    public string?   CancelledBy { get; set; }       // varchar(10) NULL
    public DateTime? DateCancelled { get; set; }     // smalldatetime NULL
    [MaxLength(255)]
    public string?   CancelRemarks { get; set; }     // varchar(255) NULL
    public bool?     PrintedOr   { get; set; }       // PrintedOR bit NULL
    public DateTime? PrintedDate { get; set; }       // smalldatetime NULL
    [MaxLength(20)]
    public string?   RefNo       { get; set; }       // varchar(20) NULL
}
