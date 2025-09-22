using Espadium.Wiki.Application.Abstractions;
using Espadium.Wiki.Application.Abstractions.Repositories;
using Espadium.Wiki.Application.DTOs;
using Espadium.Wiki.Domain.Entities;

namespace Espadium.Wiki.Application.Services
{
    public class TeamService
    {
        private readonly ITeamRepository _teams;
        private readonly IDateTimeProvider _clock;

        public TeamService(ITeamRepository teams, IDateTimeProvider clock)
        {
            _teams = teams;
            _clock = clock;
        }

        public async Task<List<TeamDto>> GetTeamsAsync()
        {
            var list = await _teams.ListAsync();
            return list.Select(t => new TeamDto(t.Id, t.Name, t.Description, t.CreatedBy, t.CreatedAt)).ToList();
        }

        public async Task<TeamDto?> GetTeamAsync(Guid id)
        {
            var team = await _teams.GetAsync(id);
            return team == null ? null : new TeamDto(team.Id, team.Name, team.Description, team.CreatedBy, team.CreatedAt);
        }

        public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, Guid userId)
        {
            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedBy = userId,
                CreatedAt = _clock.UtcNow
            };
            await _teams.AddAsync(team);

            return new TeamDto(team.Id, team.Name, team.Description, team.CreatedBy, team.CreatedAt);
        }

        public async Task<TeamDto?> UpdateTeamAsync(Guid id, UpdateTeamRequest request)
        {
            var team = await _teams.GetAsync(id);
            if (team == null) return null;

            team.Name = request.Name;
            team.Description = request.Description;
            await _teams.UpdateAsync(team);
            return new TeamDto(team.Id, team.Name, team.Description, team.CreatedBy, team.CreatedAt);
        }

        public async Task<bool> DeleteTeamAsync(Guid id)
        {
            var team = await _teams.GetAsync(id);
            if (team == null) return false;
            await _teams.DeleteAsync(team);
            return true;
        }
    }
}
