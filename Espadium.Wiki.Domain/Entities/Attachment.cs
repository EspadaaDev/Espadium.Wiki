namespace Espadium.Wiki.Domain.Entities
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public string StorageKey { get; set; } = string.Empty;
        public string Bucket { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string Mime { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? Sha256 { get; set; }
        public string Status { get; set; } = "pending"; // pending|active|blocked
        public string AvScan { get; set; } = "pending"; // pending|clean|infected
        public string? PreviewKey { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public Page? Page { get; set; }
    }
}

