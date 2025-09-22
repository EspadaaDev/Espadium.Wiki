using System.ComponentModel.DataAnnotations;

namespace Espadium.Wiki.Api.Configuration.Options
{
    public class S3Options
    {
        [Required]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        public string Bucket { get; set; } = string.Empty;

        [Required]
        public string AccessKey { get; set; } = string.Empty;

        [Required]
        public string SecretKey { get; set; } = string.Empty;

        public bool UseSsl { get; set; }
    }
}

