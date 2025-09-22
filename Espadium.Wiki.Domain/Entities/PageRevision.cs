namespace Espadium.Wiki.Domain.Entities
{
    public class PageRevision
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public int RevisionNo { get; set; }
        public Guid AuthorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string SnapshotJson { get; set; } = string.Empty;
        public string? DiffJson { get; set; }

        public Page? Page { get; set; }
    }
}
