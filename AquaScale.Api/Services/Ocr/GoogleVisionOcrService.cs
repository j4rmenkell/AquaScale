using System.Text;
using System.Text.Json;
using System.Linq; // Added for LINQ extensions (.Where)

namespace AquaScale.Api.Services.Ocr;

public class GoogleVisionOcrService : IOcrService
{
    private readonly HttpClient _http;
    private readonly ILogger<GoogleVisionOcrService> _logger;
    private readonly string _apiKey;

    public GoogleVisionOcrService(HttpClient http, ILogger<GoogleVisionOcrService> logger, IConfiguration config)
    {
        _http = http;
        _logger = logger;
        _apiKey = config["GoogleVision:ApiKey"]
            ?? throw new InvalidOperationException("GoogleVision:ApiKey not configured.");
    }

    public async Task<OcrResult> ReadMeterImageAsync(byte[] imageBytes, CancellationToken ct = default)
    {
        var base64 = Convert.ToBase64String(imageBytes);

        var requestBody = new
        {
            requests = new[]
            {
                new
                {
                    image = new { content = base64 },
                    features = new[] { new { type = "DOCUMENT_TEXT_DETECTION" } }
                }
            }
        };

        var url = $"https://vision.googleapis.com/v1/images:annotate?key={_apiKey}";
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsync(url, content, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vision API request failed to send.");
            return new OcrResult(false, "", 0, new(), $"Request failed: {ex.Message}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Vision API returned {Status}: {Body}", response.StatusCode, json);
            return new OcrResult(false, "", 0, new(), $"Vision API error {response.StatusCode}: {json}");
        }

        return ParseResponse(json);
    }

    private OcrResult ParseResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        _logger.LogInformation("RAW VISION RESPONSE: {Json}", json);
        var root = doc.RootElement;

        var annotation = root.GetProperty("responses")[0];

        if (!annotation.TryGetProperty("fullTextAnnotation", out var fullText))
        {
            return new OcrResult(true, "", 0, new(), "No text detected in image.");
        }

        var rawText = fullText.GetProperty("text").GetString() ?? "";
        var digits = new List<DigitBox>();
        var confidenceSum = 0f;
        var confidenceCount = 0;

        // pages -> blocks -> paragraphs -> words -> symbols
        foreach (var page in fullText.GetProperty("pages").EnumerateArray())
        foreach (var block in page.GetProperty("blocks").EnumerateArray())
        foreach (var paragraph in block.GetProperty("paragraphs").EnumerateArray())
        foreach (var word in paragraph.GetProperty("words").EnumerateArray())
        foreach (var symbol in word.GetProperty("symbols").EnumerateArray())
        {
            var text = symbol.GetProperty("text").GetString() ?? "";
            var confidence = symbol.TryGetProperty("confidence", out var confEl) ? confEl.GetSingle() : 0f;

            var vertices = new List<(int X, int Y)>();
            if (symbol.TryGetProperty("boundingBox", out var box) &&
                box.TryGetProperty("vertices", out var verts))
                {
                    foreach (var v in verts.EnumerateArray())
                    {
                        var x = v.TryGetProperty("x", out var xEl) ? xEl.GetInt32() : 0;
                        var y = v.TryGetProperty("y", out var yEl) ? yEl.GetInt32() : 0;
                        vertices.Add((x, y));
                    }
                }

            digits.Add(new DigitBox(text, confidence, vertices));
            confidenceSum += confidence;
            confidenceCount++;
        }

        var overallConfidence = confidenceCount > 0 ? confidenceSum / confidenceCount : 0f;

        return new OcrResult(true, rawText, overallConfidence, digits, null);
    }

    public const int BillingDigitCount = 5;
    private static readonly int[] ValidLengths = { 5, 7, 8 };

    /// <summary>
    /// Extracts exactly the first 5 digits for billing, provided the total digit 
    /// count matches our known meter types (5, 7, or 8 total digits).
    /// </summary>
    public static decimal? ExtractNumericReading(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return null;

        // Strip any non-digit noise Vision might include
        var digitsOnly = new string(rawText.Where(char.IsDigit).ToArray());

        // Safety Net: Unexpected digit count (e.g., 6 or 9)
        if (!ValidLengths.Contains(digitsOnly.Length))
        {
            // In production, we'd log this anomaly. 
            // Returning null automatically routes this to manual review.
            return null;
        }

        // The Universal Rule: First 5 digits = billing reading
        var billingPortion = digitsOnly.Substring(0, BillingDigitCount);
        
        return decimal.TryParse(billingPortion, out var value) ? value : null;
    }
}