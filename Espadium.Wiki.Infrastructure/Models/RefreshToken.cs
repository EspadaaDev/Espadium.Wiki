using Espadium.Wiki.Infrastructure.Identity;

namespace Espadium.Wiki.Infrastructure.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }

        public User? User { get; set; }
    }
}

