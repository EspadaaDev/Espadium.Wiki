using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.DTOs
{
    public record SpaceDto(Guid Id, string Key, string Name, string? Description, Guid CreatedBy, bool IsPrivate, DateTimeOffset CreatedAt);
    public record CreateSpaceRequest(string Key, string Name, string? Description, bool IsPrivate = false);
    public record UpdateSpaceRequest(string Name, string? Description, bool IsPrivate);
    public record SpaceMemberDto(Guid SpaceId, PrincipalType PrincipalType, Guid PrincipalId, SpaceRole Role);
    public record AddSpaceMemberRequest(PrincipalType PrincipalType, Guid PrincipalId, SpaceRole Role);
}
