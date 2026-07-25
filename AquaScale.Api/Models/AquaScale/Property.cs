namespace AquaScale.Api.Models.AquaScale;

public class Property
{
    public Guid Id { get; set; }

    public Guid SubdivisionId { get; set; }
    public Subdivision Subdivision { get; set; } = null!;

    public string? Block { get; set; }
    public string? Lot { get; set; }
    public string? CompPbl { get; set; } // WEBS Comp_ID+Project_ID+Block+Lot composite ref

    // Scalar FK into mirror data — NOT a navigation property.
    // Mirror tables live in a separate ownership boundary (Syncer writes,
    // Api reads); join explicitly in queries when needed, same pattern
    // proven by the meters <-> mirror_account_meters test.
    public string? MirrorAccountNo { get; set; }

    

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Meter> Meters { get; set; } = new List<Meter>();
}