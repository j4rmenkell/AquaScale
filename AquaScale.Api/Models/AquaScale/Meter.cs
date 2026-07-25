using AquaScale.Api.Models.Mirror;

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

    public Guid? MirrorAcctmtrId { get; set; }
    public MirrorAccountMeter? MirrorAccountMeter { get; set; }

    public string UtilityType { get; set; } = "Water";
    public string? QrCode { get; set; }

    // AquaScale's OWN field-reported status — intentionally distinct from
    // mirror_account_status (WEBS's Operational/Defective/Lost status,
    // per M_GenCodes Group 41, synced read-only via the Syncer).
    public MeterStatus MeterStatus { get; set; } = MeterStatus.Active;
    public DateTime? DateMarkedNonOperational { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
}