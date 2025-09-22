namespace Espadium.Wiki.Application.Abstractions
{
    public interface IFileStorage
    {
        Task<string> GetSignedReadUrlAsync(string key, TimeSpan ttl);
        Task DeleteAsync(string key);
    }
}

