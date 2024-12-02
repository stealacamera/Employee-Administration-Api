using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class ProjectMembersRepository : BaseRepository<ProjectMember>, IProjectMembersRepository
{
    public ProjectMembersRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task DeleteAllForProjectAsync(int projectId, CancellationToken cancellationToken = default)
        => await _set.Where(e => e.ProjectId == projectId)
                     .ExecuteDeleteAsync(cancellationToken);

    public async Task DeleteAllForUserAsync(int userId, CancellationToken cancellationToken = default)
        => await _set.Where(e => e.EmployeeId == userId)
                     .ExecuteDeleteAsync(cancellationToken);

    public async Task<IEnumerable<ProjectMember>> GetAllForProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.ProjectId == projectId);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProjectMember>> GetAllForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.EmployeeId == userId);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ProjectMember?> GetByIdsAsync(int userId, int projectId, CancellationToken cancellationToken = default)
        => await _set.FindAsync([projectId, userId], cancellationToken: cancellationToken);

    public async Task<bool> IsUserMemberAsync(int userId, int projectId, CancellationToken cancellationToken = default)
        => await GetByIdsAsync(userId, projectId, cancellationToken) != null;
}
