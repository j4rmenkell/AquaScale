using System.Security.Claims;
using AquaScale.Api.Authorization;
using AquaScale.Api.Data;
using AquaScale.Api.Models.AquaScale;
using AquaScale.Api.Services.Ocr;
using AquaScale.Api.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaScale.Api.Controllers.FieldOperations;

[ApiController]
[Route("api/meter-readings")]
public class MeterReadingsController : ControllerBase
{
    private readonly AquaScaleDbContext _db;
    private readonly IOcrService _ocr;
    private readonly ReadingValidationService _validator;
    private readonly IPhotoStorageService _storage; // CHANGED: added

    public MeterReadingsController(
        AquaScaleDbContext db,
        IOcrService ocr,
        ReadingValidationService validator,
        IPhotoStorageService storage) // CHANGED: added
    {
        _db = db;
        _ocr = ocr;
        _validator = validator;
        _storage = storage; // CHANGED: added
    }

    [HttpPost]
    [RequirePermission(PermissionKeys.MeterReadingsCreate)]
    
    public async Task<IActionResult> Capture([FromForm] Guid meterId, [FromForm] IFormFile image, CancellationToken ct)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !Guid.TryParse(idClaim, out var fieldWorkerId))
            return Unauthorized();

        var meter = await _db.Meters.FindAsync(new object[] { meterId }, ct);
        if (meter is null)
            return NotFound($"No meter found with id '{meterId}'.");

        using var ms = new MemoryStream();
        await image.CopyToAsync(ms, ct);
        var imageBytes = ms.ToArray(); // CHANGED: kept as a named array — needed twice now (OCR + upload)

        // CHANGED: generate the reading's ID up front, so the photo's storage key
        // can reference it before the MeterReading row itself is ever created.
        var readingId = Guid.NewGuid();

        // CHANGED: upload happens regardless of OCR outcome — even a flagged/failed
        // reading needs its photo saved, since that's exactly the case an Admin
        // needs to manually review against (per the TODO this replaces).
        var photoKey = await _storage.UploadMeterPhotoAsync(readingId, imageBytes, ct);

        var ocrResult = await _ocr.ReadMeterImageAsync(imageBytes, ct);

        if (!ocrResult.Success)
        {
            return await SaveReadingAsync(readingId, meter.Id, fieldWorkerId, null, ocrResult.OverallConfidence,
                "Flagged", $"OCR failed: {ocrResult.ErrorMessage}", photoKey, ct);
        }

        var numericReading = GoogleVisionOcrService.ExtractNumericReading(ocrResult.RawText);

        if (numericReading is null)
        {
            return await SaveReadingAsync(readingId, meter.Id, fieldWorkerId, null, ocrResult.OverallConfidence,
                "Flagged", "Could not extract a valid billing reading from OCR text.", photoKey, ct);
        }

        var validation = await _validator.ValidateAsync(meter.Id, numericReading.Value, ocrResult.OverallConfidence, ct);
        var status = validation.Decision == ReadingDecision.Approved ? "Approved" : "Flagged";

        return await SaveReadingAsync(readingId, meter.Id, fieldWorkerId, numericReading, ocrResult.OverallConfidence,
            status, validation.Reason, photoKey, ct, validation.IsSuspectedReset, validation.ResolvedPrevRead);
    }

    private async Task<IActionResult> SaveReadingAsync(
        Guid readingId, Guid meterId, Guid fieldWorkerId, decimal? ocrValue, float confidence,
        string status, string reason, string photoKey, CancellationToken ct,
        bool isSuspectedReset = false, decimal? prevRead = null)
    {
        var reading = new MeterReading
        {
            Id = readingId, // CHANGED: uses the ID generated up front in Capture, not a fresh one here
            MeterId = meterId,
            FieldWorkerId = fieldWorkerId,
            CapturedAt = DateTime.UtcNow,
            OcrReadingValue = ocrValue,
            ConfidenceScore = (decimal?)confidence,
            PreviousReading = prevRead,
            Status = status,
            // CHANGED: stores the R2 object KEY, not a URL. The bucket is private —
            // this value is meaningless on its own and must be exchanged for a
            // short-lived presigned URL (via IPhotoStorageService.GetPresignedPhotoUrl)
            // only at the moment something actually needs to display the photo.
            // Never store or return a permanent/public URL here.
            PhotoUrl = photoKey,
        };

        _db.MeterReadings.Add(reading);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            reading.Id,
            reading.Status,
            OcrReadingValue = ocrValue,
            Confidence = confidence,
            Reason = reason,
            IsSuspectedReset = isSuspectedReset,
        });
    }
}
