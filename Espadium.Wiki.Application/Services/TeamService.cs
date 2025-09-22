using Espadium.Wiki.Application.DTOs;
using Espadium.Wiki.Domain.Entities;
using Espadium.Wiki.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Espadium.Wiki.Application.Services
{
    public class TeamService
    {
        private readonly WikiDbContext _context;

        public TeamService(WikiDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeamDto>> GetTeamsAsync()
        {
            return await _context.Teams
                .Select(t => new TeamDto(t.Id, t.Name, t.Description, t.CreatedBy, t.CreatedAt))
                .ToListAsync();
        }

        public async Task<TeamDto?> GetTeamAsync(Guid id)
        {
            var team = await _context.Teams.FindAsync(id);
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
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return new TeamDto(team.Id, team.Name, team.Description, team.CreatedBy, team.CreatedAt);
        }

        public async Task<TeamDto?> UpdateTeamAsync(Guid id, UpdateTeamRequest request)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return null;

            team.Name = request.Name;
            team.Description = request.Description;

            await _context.SaveChangesAsync();
            return new TeamDto(team.Id, team.Name, team.Description, team.CreatedBy, team.CreatedAt);
        }

        public async Task<bool> DeleteTeamAsync(Guid id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return false;

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
