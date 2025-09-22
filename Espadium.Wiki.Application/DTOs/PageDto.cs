using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.DTOs
{
    public record PageDto(Guid Id, Guid SpaceId, Guid? ParentId, string Slug, string Title, PageStatus Status, Guid CreatedBy, Guid UpdatedBy, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
    public record CreatePageRequest(Guid SpaceId, Guid? ParentId, string Slug, string Title, PageStatus Status = PageStatus.Draft);
    public record UpdatePageRequest(string Title, PageStatus Status);
    public record PageRevisionDto(Guid Id, Guid PageId, int RevisionNo, Guid AuthorId, DateTimeOffset CreatedAt, string SnapshotJson, string? DiffJson);
}
