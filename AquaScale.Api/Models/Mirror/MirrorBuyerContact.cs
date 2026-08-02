namespace AquaScale.Api.Models.Mirror;

public class MirrorBuyerContact
{
    public Guid Id { get; set; }
    public string? BuyerId { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public DateTime? DateUpdated { get; set; }
}