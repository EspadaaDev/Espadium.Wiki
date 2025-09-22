using System.ComponentModel.DataAnnotations;

namespace Espadium.Wiki.Api.Configuration.Options
{
    public class EmailOptions
    {
        [Required]
        [EmailAddress]
        public string From { get; set; } = string.Empty;

        [Required]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int SmtpPort { get; set; }

        public string? SmtpUser { get; set; }
        public string? SmtpPassword { get; set; }
    }
}

