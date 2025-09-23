using System;

namespace Espadium.Wiki.Domain.Entities
{
    public class PublicLink
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool Revoked { get; set; }
    }
}

