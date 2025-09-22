using Espadium.Wiki.Application.Abstractions;

namespace Espadium.Wiki.Infrastructure.Services
{
    public class S3Storage : IFileStorage
    {
        public Task<string> GetSignedReadUrlAsync(string key, TimeSpan ttl)
        {
            return Task.FromResult(string.Empty);
        }

        public Task DeleteAsync(string key)
        {
            return Task.CompletedTask;
        }
    }
}

