using System.ComponentModel.DataAnnotations;

namespace AquaScale.Api.Models.Webs;

/// <summary>
/// Maps to WEBS.dbo.M_GenCodes — general code lookup table.
/// Composite PK: Group_Code + Field_Code (both char 2).
/// Used to decode status codes, meter status, BackoutType reason, etc.
/// Example: Group 41 = MeterStatus codes (AC=Active, DF=Defective, LS=Lost, etc.)
/// Schema validated against WEBS-Schema.txt.
/// </summary>
public class WEBSGenCodes
{
    [MaxLength(2)]
    public string  GroupCode    { get; set; } = null!; // Group_Code char(2) NOT NULL — composite PK
    [MaxLength(2)]
    public string  FieldCode    { get; set; } = null!; // Field_Code char(2) NOT NULL — composite PK
    [MaxLength(50)]
    public string  Description  { get; set; } = null!; // nvarchar(50) NOT NULL
    public bool?   Saleable     { get; set; }           // bit NULL
    [MaxLength(1)]
    public string? Status_ID    { get; set; }           // char(1) NULL
    [MaxLength(10)]
    public string? ShortDesc    { get; set; }           // varchar(10) NULL
}
