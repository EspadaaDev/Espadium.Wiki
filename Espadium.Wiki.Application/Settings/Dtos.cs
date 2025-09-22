namespace Espadium.Wiki.Application.Settings
{
    public record SystemSettingsDto(
        StorageLimitsDto StorageLimits,
        ContentLimitsDto ContentLimits,
        SecurityDto Security,
        S3PublicDto S3,
        EmailPublicDto Email);

    public record UpdateSystemSettingsDto(
        StorageLimitsDto StorageLimits,
        ContentLimitsDto ContentLimits,
        SecurityDto Security,
        S3UpdateDto S3,
        EmailUpdateDto Email);

    public record StorageLimitsDto(int MaxFileSizeMB, string[] AllowedMimeTypes, bool UsePresignedUrls, int MultipartUploadThresholdMB);
    public record ContentLimitsDto(int MaxPageLengthChars, int MaxTitleLength, int MaxRevisionsPerPage);
    public record SecurityDto(bool PublicLinksEnabled, int DefaultPublicLinkTTLHours, bool RequireEmailVerification);
    public record S3PublicDto(string Endpoint, string Bucket, bool UseSsl);
    public record EmailPublicDto(string From, string SmtpHost, int SmtpPort);
    public record S3UpdateDto(string Endpoint, string Bucket, bool UseSsl, string? AccessKey, string? SecretKey);
    public record EmailUpdateDto(string From, string SmtpHost, int SmtpPort, string? SmtpUser, string? SmtpPassword);
}

