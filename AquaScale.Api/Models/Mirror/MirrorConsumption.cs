namespace AquaScale.Api.Models.Mirror;

public class MirrorConsumption
{
    public Guid Id { get; set; }
    public Guid? AcctMtrId { get; set; }
    public double? CurRead { get; set; }
    public double? PrevRead { get; set; }
    public DateTime? DateRead { get; set; }
}