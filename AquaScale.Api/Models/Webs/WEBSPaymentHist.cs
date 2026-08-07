using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.T_Payment_Hist — payment deletion/void history.
/// No PK declared in WEBS schema (IsPrimaryKey=0 for all columns).
/// ID + DeleteCounter is the logical composite key configured in WebsDbContext.
/// Note: T_Payment_Hist uses datetime for ORDate/DepoDate/DueDate (unlike T_Payment which uses date).
/// Schema validated against WEBS-Schema.txt.
/// </summary>
public class WEBSPaymentHist
{
    public Guid      Id            { get; set; }     // uniqueidentifier NOT NULL (part of logical key)
    public DateTime? DateDeleted   { get; set; }     // datetime NULL
    public int       DeleteCounter { get; set; }     // int NOT NULL (part of logical key)
    public Guid      AcctMtrId     { get; set; }     // AcctMtr_ID uniqueidentifier NOT NULL
    public int       SeqNo         { get; set; }     // int NOT NULL
    public DateTime  OrDate        { get; set; }     // ORDate datetime NOT NULL (datetime here, not date!)
    public DateTime  DepoDate      { get; set; }     // datetime NOT NULL
    [MaxLength(10)]
    public string    BankDepo      { get; set; } = null!; // nvarchar(10) NOT NULL
    [MaxLength(20)]
    public string    BankAcctNo    { get; set; } = null!; // nchar(20) NOT NULL
    public DateTime  DueDate       { get; set; }     // datetime NOT NULL
    [MaxLength(10)]
    public string    OrNo          { get; set; } = null!; // ORNo nvarchar(10) NOT NULL
    public decimal?  CurCharge     { get; set; }     // money NULL
    public decimal?  Penalty       { get; set; }     // money NULL
    public decimal   TotAmtDue     { get; set; }     // money NOT NULL
    public decimal   AmtPaid       { get; set; }     // money NOT NULL
    public decimal   Balance       { get; set; }     // money NOT NULL
    public decimal?  PrevBalance   { get; set; }     // money NULL
    public decimal?  PrevPenalty   { get; set; }     // money NULL
    [MaxLength(2)]
    public string?   WaivePenalty  { get; set; }     // nvarchar(2) NULL
    [MaxLength(500)]
    public string?   Remarks       { get; set; }     // nvarchar(500) NULL
    public Guid?     ConsumptionId { get; set; }     // ConsumptionID uniqueidentifier NULL
    [MaxLength(10)]
    public string?   GlRefNo       { get; set; }     // GLRefNo nchar(10) NULL
    [MaxLength(1)]
    public string?   RecType       { get; set; }     // nchar(1) NULL
    [MaxLength(10)]
    public string?   UpdatedBy     { get; set; }     // nchar(10) NULL
    public DateTime? DateUpdated   { get; set; }     // datetime NULL
    public bool?     PrintedOr     { get; set; }     // PrintedOR bit NULL
    public DateTime? PrintedDate   { get; set; }     // smalldatetime NULL
    [MaxLength(20)]
    public string?   RefNo         { get; set; }     // varchar(20) NULL
}
