namespace Espadium.Wiki.Domain.Entities
{
    public class TeamMember
    {
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;

        public Team? Team { get; set; }
    }
}
