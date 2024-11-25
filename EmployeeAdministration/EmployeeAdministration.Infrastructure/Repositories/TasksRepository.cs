using EmployeeAdministration.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class TasksRepository : Repository<Domain.Entities.Task>, ITasksRepository
{
    public TasksRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task DeleteAllForProjectAsync(int projectId, CancellationToken cancellationToken = default)
        => _set.Where(e => e.ProjectId == projectId)
               .ExecuteDeleteAsync(cancellationToken);

    public async Task<bool> DoesProjectHaveOpenTasksAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.ProjectId == projectId);
        query = query.Where(e => !e.IsCompleted);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> DoesUserHaveOpenTasksAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.AppointeeEmployeeId == userId && !e.IsCompleted);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> DoesUserHaveOpenTasksAsync(int userId, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.AppointeeEmployeeId == userId);

        if (projectId.HasValue)
            query = query.Where(e => e.ProjectId == projectId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetAllForProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.ProjectId == projectId);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Task>> GetAllForUserAsync(int userId, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.AppointeeEmployeeId == userId);

        if (projectId.HasValue)
            query = query.Where(e => e.ProjectId == projectId.Value);

        return await query.ToListAsync(cancellationToken);
    }
}
