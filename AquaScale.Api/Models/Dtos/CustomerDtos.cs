namespace AquaScale.Api.Models.Dtos;

public record CustomerListItemDto(
    Guid MeterId,
    string SubdivisionName,
    string? AccountNo,
    string? Block,
    string? Lot,
    string? CustomerName,
    string AccountStatus, // "Active" | "New Customer" — see assumption above, confirm
    string UtilityType,
    string MeterStatusCode, // raw WEBS code for now — decode pending M_GenCodes confirmation
    string? MeterStatusLabel // null until decode map is confirmed
);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);