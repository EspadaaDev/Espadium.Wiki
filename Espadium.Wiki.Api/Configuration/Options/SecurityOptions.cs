using System.ComponentModel.DataAnnotations;

namespace Espadium.Wiki.Api.Configuration.Options
{
    public class SecurityOptions
    {
        public bool PublicLinksEnabled { get; set; }

        [Range(1, int.MaxValue)]
        public int DefaultPublicLinkTTLHours { get; set; }

        public bool RequireEmailVerification { get; set; }
    }
}

