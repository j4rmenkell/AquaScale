namespace AquaScale.Api.Models.AquaScale;

public class MeterReading
{
    public Guid Id { get; set; }

    public Guid MeterId { get; set; }
    public Meter Meter { get; set; } = null!;

    public Guid FieldWorkerId { get; set; }
    public Profile FieldWorker { get; set; } = null!;

    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    // Geotagging (per panel revision — geo-tag, not geo-fence)
    public decimal? GpsLat { get; set; }
    public decimal? GpsLng { get; set; }
    public int? SecondsSinceLastCapture { get; set; }

    public string? PhotoUrl { get; set; } // Cloudflare R2 URL
    public string? QrScannedCode { get; set; }

    public decimal? OcrReadingValue { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public decimal? PreviousReading { get; set; }

    public int RecaptureCount { get; set; } = 0;
    public bool IsDuplicateFlag { get; set; } = false;

    public string Status { get; set; } = "Pending"; // e.g. Pending, Reviewed, Flagged

    public Guid? ReviewedBy { get; set; }
    public Profile? Reviewer { get; set; }
    public DateTime? ReviewedAt { get; set; }
}