using Espadium.Wiki.Application.Settings;

namespace Espadium.Wiki.Application.Abstractions.Settings
{
    public interface ISettingsService
    {
        Task<SystemSettingsDto> GetAsync(CancellationToken ct);
        Task<SystemSettingsDto> UpdateAsync(UpdateSystemSettingsDto dto, Guid actorId, CancellationToken ct);
    }
}

