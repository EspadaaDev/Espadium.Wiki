using System.ComponentModel.DataAnnotations;

namespace Espadium.Wiki.Api.Configuration.Options
{
    public class StorageLimitsOptions
    {
        [Range(1, int.MaxValue)]
        public int MaxFileSizeMB { get; set; }

        [Required]
        public string[] AllowedMimeTypes { get; set; } = Array.Empty<string>();

        public bool UsePresignedUrls { get; set; }

        [Range(1, int.MaxValue)]
        public int MultipartUploadThresholdMB { get; set; }
    }
}

