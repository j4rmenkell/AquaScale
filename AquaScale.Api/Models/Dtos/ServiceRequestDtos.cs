namespace AquaScale.Api.Models.Dtos;

public record AssignedToDto(
    Guid Id,
    string FullName,
    string? RoleName
);

public record ServiceRequestListItemDto(
    Guid Id,
    string? AccountNumber,
    string? AccountName,
    string Subdivision,
    string? Block,
    string? Lot,
    string IssueType,
    string? MeterType,
    string Status,
    AssignedToDto? AssignedTo,
    DateTime DateReported,
    bool HasAttachedImage,
    string? HomeownerMessage,
    string? InternalNotes
);

public record ServiceRequestDetailDto(
    Guid Id,
    Guid PropertyId,
    Guid? MeterId,
    Guid? CustomerId,
    string? AccountNumber,
    string? AccountName,
    string Subdivision,
    string? Block,
    string? Lot,
    string? CompPbl,
    string? CustomerContactNo,
    string? CustomerEmail,
    string IssueType,
    string? MeterType,
    string Status,
    AssignedToDto? AssignedTo,
    DateTime DateReported,
    DateTime? ResolvedAt,
    bool HasAttachedImage,
    string? ImageUrl,
    string? HomeownerMessage,
    string? InternalNotes
);

public class UpdateServiceRequestAssignmentDto
{
    public Guid? AssignedToProfileId { get; set; }
    public string? InternalNotes { get; set; }
}

public class UpdateServiceRequestStatusDto
{
    public string Status { get; set; } = null!;
    public string? InternalNotes { get; set; }
}

public class CreateServiceRequestDto
{
    public Guid PropertyId { get; set; }
    public Guid? MeterId { get; set; }
    public Guid? CustomerId { get; set; }
    public string IssueType { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
