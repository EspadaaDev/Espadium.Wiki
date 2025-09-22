using System.ComponentModel.DataAnnotations;

namespace Espadium.Wiki.Api.Configuration.Options
{
    public class JwtOptions
    {
        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Required]
        [MinLength(16)]
        public string Key { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int AccessTokenMinutes { get; set; }

        [Range(1, int.MaxValue)]
        public int RefreshTokenDays { get; set; }
    }
}

