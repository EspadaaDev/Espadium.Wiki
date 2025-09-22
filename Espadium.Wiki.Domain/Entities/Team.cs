namespace Espadium.Wiki.Domain.Entities
{
    public class Team
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public List<TeamMember> Members { get; set; } = [];
    }
}
