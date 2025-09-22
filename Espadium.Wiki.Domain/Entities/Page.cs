namespace Espadium.Wiki.Domain.Entities
{
    public enum PageStatus
    {
        Published,
        Draft,
        Archived
    }

    public class Page
    {
        public Guid Id { get; set; }
        public Guid SpaceId { get; set; }
        public Guid? ParentId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public PageStatus Status { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public Space? Space { get; set; }
        public Page? Parent { get; set; }
        public List<Page> Children { get; set; } = [];
        public List<PageRevision> Revisions { get; set; } = [];
    }
}
