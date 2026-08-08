using AquaScale.Api.Data;
using AquaScale.Api.Models.Webs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AquaScale.Api.Services.Ocr;

public enum ReadingDecision { Approved, Flagged }

public record ValidationResult(
    ReadingDecision Decision,
    string Reason,
    bool IsSuspectedReset,
    decimal? ResolvedPrevRead,
    decimal? MaxPlausibleUsage);

public class ReadingValidationService
{
    private readonly AquaScaleDbContext _db;
    private readonly WebsDbContext      _webs;
    private readonly ILogger<ReadingValidationService> _logger;

    private const bool    ConfidenceThresholdEnabled = true;
    private const float   ConfidenceThreshold        = 0.85f;
    private const decimal DefaultMultiplier          = 3.0m;
    private const decimal Tier3FlatDefault           = 33m;

    public ReadingValidationService(AquaScaleDbContext db, WebsDbContext webs, ILogger<ReadingValidationService> logger)
    {
        _db     = db;
        _webs   = webs;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateAsync(
        Guid meterId, decimal rawReading, float overallConfidence, CancellationToken ct = default)
    {
        if (ConfidenceThresholdEnabled && overallConfidence < ConfidenceThreshold)
        {
            return new ValidationResult(ReadingDecision.Flagged,
                $"Confidence {overallConfidence:F2} below threshold {ConfidenceThreshold:F2}",
                false, null, null);
        }

        var prevRead = await GetPrevReadAsync(meterId, ct);
        if (prevRead is null)
        {
            return new ValidationResult(ReadingDecision.Flagged,
                "First-ever reading for this meter — no baseline exists to validate plausibility against.",
                false, null, null);
        }

        if (rawReading < prevRead)
        {
            return new ValidationResult(ReadingDecision.Flagged,
                "Reading is lower than previous reading.", true, prevRead, null);
        }

        var maxPlausibleUsage = await GetMaxPlausibleUsageAsync(meterId, ct);
        var upperBound = prevRead.Value + maxPlausibleUsage;

        if (rawReading > upperBound)
        {
            return new ValidationResult(ReadingDecision.Flagged,
                $"Reading {rawReading} exceeds plausible range (prev {prevRead} + max usage {maxPlausibleUsage} = {upperBound}).",
                false, prevRead, maxPlausibleUsage);
        }

        return new ValidationResult(ReadingDecision.Approved, "Within plausible range.",
            false, prevRead, maxPlausibleUsage);
    }

    private async Task<decimal?> GetPrevReadAsync(Guid meterId, CancellationToken ct)
    {
        // Priority 1: use the most recent AquaScale OCR reading for this meter
        var lastAquaScaleReading = await _db.MeterReadings
            .Where(mr => mr.MeterId == meterId && mr.OcrReadingValue != null)
            .OrderByDescending(mr => mr.CapturedAt)
            .Select(mr => mr.OcrReadingValue)
            .FirstOrDefaultAsync(ct);

        if (lastAquaScaleReading is not null) return lastAquaScaleReading;

        // Priority 2: fall back to the last WEBS CurRead (historical WEBS billing)
        var meter = await _db.Meters.FindAsync(new object[] { meterId }, ct);
        if (meter?.MirrorAcctmtrId is null) return null;

        var websLastRead = await _webs.Consumptions
            .Where(c => c.AcctMtrId == meter.MirrorAcctmtrId)
            .OrderByDescending(c => c.DateRead)
            .Select(c => c.CurRead)
            .FirstOrDefaultAsync(ct);

        return websLastRead != 0 ? (decimal)websLastRead : null;
    }

    private async Task<decimal> GetMaxPlausibleUsageAsync(Guid meterId, CancellationToken ct)
    {
        var meter = await _db.Meters.FindAsync(new object[] { meterId }, ct);
        _logger.LogInformation("GetMaxPlausibleUsage: meterId={MeterId}, meterFound={Found}, mirrorAcctmtrId={MirrorId}",
            meterId, meter != null, meter?.MirrorAcctmtrId);

        if (meter?.MirrorAcctmtrId is null) return Tier3FlatDefault;

        var rawHistory = await _webs.Consumptions
            .Where(c => c.AcctMtrId == meter.MirrorAcctmtrId)
            .OrderByDescending(c => c.DateRead)
            .Take(12)
            .Select(r => new { r.CurRead, r.PrevRead })
            .ToListAsync(ct);

        _logger.LogInformation("GetMaxPlausibleUsage: rawHistory.Count={Count}", rawHistory.Count);

        var usageHistory = rawHistory
            .Select(r => (decimal)r.CurRead - (decimal)r.PrevRead)
            .ToList();

        _logger.LogInformation("GetMaxPlausibleUsage: usageHistory.Count={Count}, values=[{Values}]",
            usageHistory.Count, string.Join(",", usageHistory));

        if (usageHistory.Count >= 3)
        {
            var sorted       = usageHistory.OrderBy(x => x).ToList();
            var median       = sorted[sorted.Count / 2];
            var historicalMax = sorted[^1];

            var effectiveBase = median > 0 ? median : historicalMax * 0.5m;
            return effectiveBase * DefaultMultiplier;
        }

        return Tier3FlatDefault;
    }
}