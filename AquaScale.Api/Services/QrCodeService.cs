using QRCoder;

namespace AquaScale.Api.Services.Storage;

public interface IQrCodeService
{
    byte[] GenerateQrCode(Guid meterId);
}

public class QrCodeService : IQrCodeService
{
    /// <summary>
    /// Encodes only the Meter.Id — confirmed as the correct lookup key after
    /// verifying real WEBS data: a property has exactly one ACTIVE meter at a
    /// time (multiple mirror_account_meters rows per property_id reflect
    /// replacement history, not concurrent meters — confirmed via real
    /// meter_status codes, e.g. account W-08-00520: one '02' retired 2012,
    /// one '01' active). ID-based lookup avoids depending on meter_status
    /// being correct at scan time, and requires no join.
    ///
    /// Block/lot/subdivision are NOT encoded here — they're printed as
    /// human-readable label text on the physical sticker for the Field
    /// Worker's visual confirmation, kept separate from the QR's machine-
    /// readable payload.
    /// </summary>
    public byte[] GenerateQrCode(Guid meterId)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(
            meterId.ToString(), QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20); // 20px per module, reasonably print-ready
    }
}