namespace Espadium.Wiki.Domain.Entities
{
    public class AdminSettingsAudit
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public DateTimeOffset ChangedAt { get; set; }
        public Guid ChangedBy { get; set; }
    }
}

