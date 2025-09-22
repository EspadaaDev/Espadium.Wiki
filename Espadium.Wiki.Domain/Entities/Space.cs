namespace Espadium.Wiki.Domain.Entities
{
    public class Space
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsPrivate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public List<SpaceMember> Members { get; set; } = [];
        public List<Page> Pages { get; set; } = [];
    }
}
