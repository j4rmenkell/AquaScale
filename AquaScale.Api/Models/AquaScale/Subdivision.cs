namespace AquaScale.Api.Models.AquaScale;

public class Subdivision
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? LegacyCode { get; set; } // WEBS Project_ID — traces seeded subdivisions back to source
    public string? GeojsonBoundary { get; set; } // Turf.js geofence boundary
    public string? MobileDataProvider { get; set; }
    public bool IsActive { get; set; } = true; // Fig 3.11 activation/deactivation toggle
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}