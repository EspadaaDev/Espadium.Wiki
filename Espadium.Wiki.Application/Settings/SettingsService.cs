using System.Text.Json;
using Espadium.Wiki.Application.Abstractions.Caching;
using Espadium.Wiki.Application.Abstractions.Settings;

namespace Espadium.Wiki.Application.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsStore _store;
        private readonly ISettingsCache _cache;
        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web) { WriteIndented = false };

        public SettingsService(ISettingsStore store, ISettingsCache cache)
        {
            _store = store;
            _cache = cache;
        }

        public async Task<SystemSettingsDto> GetAsync(CancellationToken ct)
        {
            var cached = await _cache.GetAsync(ct);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<SystemSettingsDto>(cached, JsonOpts)!;
            }
            var dict = await _store.GetAllAsync(ct);
            var dto = FromDict(dict);
            await _cache.SetAsync(JsonSerializer.Serialize(dto, JsonOpts), TimeSpan.FromSeconds(60), ct);
            return dto;
        }

        public async Task<SystemSettingsDto> UpdateAsync(UpdateSystemSettingsDto dto, Guid actorId, CancellationToken ct)
        {
            Validate(dto);
            var dict = ToDict(dto);
            await _store.BulkUpsertAsync(dict, actorId, ct);
            await _cache.InvalidateAsync(ct);
            return await GetAsync(ct);
        }

        private static SystemSettingsDto FromDict(Dictionary<string, string> kv)
        {
            T Get<T>(string key, T def) => kv.TryGetValue(key, out var v) ? JsonSerializer.Deserialize<T>(v, JsonOpts)! : def;
            var storage = Get("storage", new StorageLimitsDto(50, new[] { "image/*", "application/pdf", "text/plain", "text/csv", "application/vnd.openxmlformats-officedocument.*" }, true, 8));
            var content = Get("content", new ContentLimitsDto(100000, 200, 50));
            var security = Get("security", new SecurityDto(true, 168, true));
            var s3 = Get("s3", new S3PublicDto("http://localhost:9000", "wiki", false));
            var email = Get("email", new EmailPublicDto("no-reply@example.local", "localhost", 2525));
            return new SystemSettingsDto(storage, content, security, s3, email);
        }

        private static Dictionary<string, string> ToDict(UpdateSystemSettingsDto dto)
        {
            var dict = new Dictionary<string, string>
            {
                ["storage"] = JsonSerializer.Serialize(dto.StorageLimits, JsonOpts),
                ["content"] = JsonSerializer.Serialize(dto.ContentLimits, JsonOpts),
                ["security"] = JsonSerializer.Serialize(dto.Security, JsonOpts),
                ["s3"] = JsonSerializer.Serialize(new S3PublicDto(dto.S3.Endpoint, dto.S3.Bucket, dto.S3.UseSsl), JsonOpts),
                ["email"] = JsonSerializer.Serialize(new EmailPublicDto(dto.Email.From, dto.Email.SmtpHost, dto.Email.SmtpPort), JsonOpts)
            };
            if (!string.IsNullOrEmpty(dto.S3.AccessKey)) dict["s3.accessKey"] = JsonSerializer.Serialize(dto.S3.AccessKey, JsonOpts);
            if (!string.IsNullOrEmpty(dto.S3.SecretKey)) dict["s3.secretKey"] = JsonSerializer.Serialize(dto.S3.SecretKey, JsonOpts);
            if (!string.IsNullOrEmpty(dto.Email.SmtpUser)) dict["email.smtpUser"] = JsonSerializer.Serialize(dto.Email.SmtpUser, JsonOpts);
            if (!string.IsNullOrEmpty(dto.Email.SmtpPassword)) dict["email.smtpPassword"] = JsonSerializer.Serialize(dto.Email.SmtpPassword, JsonOpts);
            return dict;
        }

        private static void Validate(UpdateSystemSettingsDto dto)
        {
            if (dto.StorageLimits.MaxFileSizeMB < 1 || dto.StorageLimits.MaxFileSizeMB > 2000) throw new ArgumentOutOfRangeException(nameof(dto.StorageLimits.MaxFileSizeMB));
            if (dto.StorageLimits.MultipartUploadThresholdMB < 5 || dto.StorageLimits.MultipartUploadThresholdMB > 128) throw new ArgumentOutOfRangeException(nameof(dto.StorageLimits.MultipartUploadThresholdMB));
            if (dto.ContentLimits.MaxPageLengthChars < 1000 || dto.ContentLimits.MaxPageLengthChars > 500000) throw new ArgumentOutOfRangeException(nameof(dto.ContentLimits.MaxPageLengthChars));
            if (dto.ContentLimits.MaxTitleLength < 1 || dto.ContentLimits.MaxTitleLength > 300) throw new ArgumentOutOfRangeException(nameof(dto.ContentLimits.MaxTitleLength));
            if (dto.ContentLimits.MaxRevisionsPerPage < 1 || dto.ContentLimits.MaxRevisionsPerPage > 1000) throw new ArgumentOutOfRangeException(nameof(dto.ContentLimits.MaxRevisionsPerPage));
            if (dto.Security.DefaultPublicLinkTTLHours < 1 || dto.Security.DefaultPublicLinkTTLHours > 720) throw new ArgumentOutOfRangeException(nameof(dto.Security.DefaultPublicLinkTTLHours));
            if (string.IsNullOrWhiteSpace(dto.S3.Endpoint) || string.IsNullOrWhiteSpace(dto.S3.Bucket)) throw new ArgumentException("S3 endpoint/bucket");
            if (string.IsNullOrWhiteSpace(dto.Email.From) || string.IsNullOrWhiteSpace(dto.Email.SmtpHost)) throw new ArgumentException("Email from/host");
            if (dto.Email.SmtpPort < 1 || dto.Email.SmtpPort > 65535) throw new ArgumentOutOfRangeException(nameof(dto.Email.SmtpPort));
            if (dto.StorageLimits.AllowedMimeTypes is null || dto.StorageLimits.AllowedMimeTypes.Length == 0) throw new ArgumentException("AllowedMimeTypes");
        }
    }
}

