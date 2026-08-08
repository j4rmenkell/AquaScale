namespace AquaScale.Api.Models.AquaScale;

public class ServiceRequest
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public Guid? MeterId { get; set; }
    public Meter? Meter { get; set; }

    public Guid? CustomerId { get; set; }
    public Profile? Customer { get; set; }

    public string IssueType { get; set; } = null!; // e.g. "Leak", "Billing Dispute", "Meter Concern"
    public string? Description { get; set; } // Homeowner's message

    public string Status { get; set; } = "Open"; // "Open", "In Progress", "Needs Field Visit", "Resolved"

    public Guid? AssignedTo { get; set; }
    public Profile? AssignedToProfile { get; set; }

    public string? InternalNotes { get; set; }
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
