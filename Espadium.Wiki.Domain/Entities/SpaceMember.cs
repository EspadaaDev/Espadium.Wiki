namespace Espadium.Wiki.Domain.Entities
{
    public enum PrincipalType
    {
        User,
        Team
    }

    public enum SpaceRole
    {
        Viewer,
        Contributor,
        Admin
    }

    public class SpaceMember
    {
        public Guid SpaceId { get; set; }
        public PrincipalType PrincipalType { get; set; }
        public Guid PrincipalId { get; set; }
        public SpaceRole Role { get; set; }

        public Space? Space { get; set; }
    }
}
