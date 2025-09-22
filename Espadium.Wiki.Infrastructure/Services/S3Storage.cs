using Espadium.Wiki.Application.Abstractions;

namespace Espadium.Wiki.Infrastructure.Services
{
    public class S3Storage : IFileStorage
    {
        public Task<PresignInitResult> InitAsync(string objectKey, long totalSizeBytes, int? partSizeBytes = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CompleteAsync(string objectKey, string uploadId, IEnumerable<(int partNumber, string etag)> parts, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSignedReadUrlAsync(string objectKey, TimeSpan ttl, CancellationToken ct = default)
            => Task.FromResult(string.Empty);

        public Task DeleteAsync(string objectKey, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}

