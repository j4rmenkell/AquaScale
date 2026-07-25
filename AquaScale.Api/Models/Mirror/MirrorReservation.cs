namespace AquaScale.Api.Models.Mirror;

public class MirrorReservation
{
    public Guid Id { get; set; }
    public string? CompPbl { get; set; }
    public string? BuyerId { get; set; }
    public DateTime? DateReserved { get; set; }
    public string? BackoutType { get; set; }
}