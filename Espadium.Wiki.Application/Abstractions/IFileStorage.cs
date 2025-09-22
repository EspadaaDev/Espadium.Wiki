namespace Espadium.Wiki.Application.Abstractions
{
    public record PresignedPart(int PartNumber, string Url);
    public record PresignInitResult(string UploadId, int PartSizeBytes, IEnumerable<PresignedPart> Parts);

    public interface IFileStorage
    {
        Task<PresignInitResult> InitAsync(string objectKey, long totalSizeBytes, int? partSizeBytes = null, System.Threading.CancellationToken ct = default);
        Task CompleteAsync(string objectKey, string uploadId, IEnumerable<(int partNumber, string etag)> parts, System.Threading.CancellationToken ct = default);
        Task<string> GetSignedReadUrlAsync(string objectKey, TimeSpan ttl, System.Threading.CancellationToken ct = default);
        Task DeleteAsync(string objectKey, System.Threading.CancellationToken ct = default);
    }
}

