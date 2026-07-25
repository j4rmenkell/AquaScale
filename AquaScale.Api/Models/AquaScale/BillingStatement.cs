using System;
using System.Collections.Generic;

namespace AquaScale.Api.Models.AquaScale;

public partial class BillingStatement
{
    public Guid Id { get; set; }

    public Guid? MeterReadingId { get; set; }

    public Guid? PropertyId { get; set; }

    public string UtilityType { get; set; } = null!;

    public decimal CurrentCharge { get; set; }

    public decimal? PreviousBalance { get; set; }

    public decimal? Penalty { get; set; }

    public decimal TotalAmountDue { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public DateTime? VoidedAt { get; set; }

    public Guid? SupersededBy { get; set; }

    public DateTime? DueDate { get; set; }

    public string WebsSyncStatus { get; set; } = null!;

    public Guid? WebsConsumptionId { get; set; }

    public string? WebsSyncError { get; set; }

    public int? WebsSeqNo { get; set; }
}
