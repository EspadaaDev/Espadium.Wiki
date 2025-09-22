namespace Espadium.Wiki.Application.DTOs
{
    public record TeamDto(Guid Id, string Name, string? Description, Guid CreatedBy, DateTimeOffset CreatedAt);
    public record CreateTeamRequest(string Name, string? Description);
    public record UpdateTeamRequest(string Name, string? Description);
}
