namespace AquaScale.Api.Services.Ocr;

public record DigitBox(string Text, float Confidence, List<(int X, int Y)> Vertices);

public record OcrResult(
    bool Success,
    string RawText,
    float OverallConfidence,
    List<DigitBox> Digits,
    string? ErrorMessage);

public interface IOcrService
{
    Task<OcrResult> ReadMeterImageAsync(byte[] imageBytes, CancellationToken ct = default);
}