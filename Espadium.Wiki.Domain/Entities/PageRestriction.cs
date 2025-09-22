namespace Espadium.Wiki.Domain.Entities
{
    public class PageRestriction
    {
        public Guid PageId { get; set; }
        public Guid UserId { get; set; }
        public bool CanEdit { get; set; }

        public Page? Page { get; set; }
    }
}

