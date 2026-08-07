
namespace AquaScale.Api.Models.AquaScale;

public enum MeterStatus
{
    Active,
    NonOperational
}

public class Meter
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    // Scalar reference to WEBS.dbo.T_Account_Meter.ID.
    // No EF navigation property — cross-database FKs are not DB-enforced.
    // Query WEBSAccountMeters in DbContext to resolve the account-meter record.
    public Guid? MirrorAcctmtrId { get; set; }

    public string UtilityType { get; set; } = "Water";
    public string? QrCode { get; set; }

    // AquaScale's OWN field-reported status — intentionally distinct from
    // T_Account_Meter_Status (WEBS's Operational/Defective/Lost status,
    // per M_GenCodes Group 41, queried read-only from WEBS).
    public MeterStatus MeterStatus { get; set; } = MeterStatus.Active;
    public DateTime? DateMarkedNonOperational { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
}