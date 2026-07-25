using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaScale.Api.Models.Mirror;

/// <summary>
/// Read-only mirror of mirror_account_meters. AquaScale.Syncer owns all writes here —
/// this context must NEVER call SaveChanges() against this entity.
/// Keep this shape in lockstep with AquaScale.Syncer/Models/Mirror/MirrorAccountMeter.cs.
/// </summary>
[Table("mirror_account_meters")]
public class MirrorAccountMeter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public string? AccountNo { get; set; }
    public string? MeterNo { get; set; }
    public DateTime? DateInstalled { get; set; }
    public decimal? DepAmt { get; set; }
    public DateTime? DepDate { get; set; }
    public string? MeterStatus { get; set; }
    public DateTime? StatusDate { get; set; }
    public string? Remarks { get; set; }
    public string? Notes { get; set; }
    public string? Createdby { get; set; }
    public DateTime? DateCreated { get; set; }
    public string? Editedby { get; set; }
    public DateTime? DateEdited { get; set; }
    public DateTime SyncedAt { get; set; }
}