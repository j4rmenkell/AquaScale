using Amazon.S3;
using Amazon.S3.Model;

namespace AquaScale.Api.Services.Storage;

public interface IPhotoStorageService
{
    Task<string> UploadMeterPhotoAsync(Guid meterReadingId, byte[] imageBytes, CancellationToken ct = default);
    string GetPresignedPhotoUrl(string photoKey, TimeSpan? expiry = null);
}

public class R2StorageService : IPhotoStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public R2StorageService(IConfiguration config)
    {
        _bucketName = config["CloudflareR2:BucketName"]
            ?? throw new InvalidOperationException("CloudflareR2:BucketName not configured.");
        var accountId = config["CloudflareR2:AccountId"]
            ?? throw new InvalidOperationException("CloudflareR2:AccountId not configured.");
        var accessKey = config["CloudflareR2:AccessKeyId"]
            ?? throw new InvalidOperationException("CloudflareR2:AccessKeyId not configured.");
        var secretKey = config["CloudflareR2:SecretAccessKey"]
            ?? throw new InvalidOperationException("CloudflareR2:SecretAccessKey not configured.");

        _s3 = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
            RequestChecksumCalculation = Amazon.Runtime.RequestChecksumCalculation.WHEN_REQUIRED,
        });
    }

    public async Task<string> UploadMeterPhotoAsync(Guid meterReadingId, byte[] imageBytes, CancellationToken ct = default)
    {
        var key = $"meter-readings/{meterReadingId}.jpg";

        using var stream = new MemoryStream(imageBytes);
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = "image/jpeg",
            // CHANGED: R2 doesn't support ANY AWS4 chunked/streaming payload
            // signature variant — only a single, fully-buffered signed
            // payload. This is the correct, documented fix for the
            // "STREAMING-AWS4-HMAC-SHA256-PAYLOAD not implemented" error.
            UseChunkEncoding = false,
            DisablePayloadSigning = true,
        }, ct);

        // Returns just the KEY, not a URL — bucket stays private (default, safest
        // setting). A viewable link only ever gets minted on-demand via
        // GetPresignedPhotoUrl, never stored permanently.
        return key;
    }

    public string GetPresignedPhotoUrl(string photoKey, TimeSpan? expiry = null)
    {
        // Call this ONLY at the moment something actually needs to display the
        // photo (e.g. a GET endpoint returning a MeterReading's details to the
        // frontend) — never persist the result. Matches the project's existing
        // rule that the frontend never talks to storage directly; access is
        // always mediated, per-request, through AquaScale.Api's own auth.
        return _s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = photoKey,
            Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(15)),
        });
    }
}