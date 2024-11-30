using EmployeeAdministration.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using Task = EmployeeAdministration.Domain.Entities.Task;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class TasksRepository : Repository<Task>, ITasksRepository
{
    public TasksRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public System.Threading.Tasks.Task DeleteAllForProjectAsync(int projectId, CancellationToken cancellationToken = default)
        => _set.Where(e => e.ProjectId == projectId)
               .ExecuteDeleteAsync(cancellationToken);

    public async Task<bool> DoesProjectHaveOpenTasksAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.ProjectId == projectId);
        query = query.Where(e => !e.IsCompleted);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> DoesUserHaveOpenTasksAsync(int userId, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.AppointeeEmployeeId == userId);
        query = query.Where(e => !e.IsCompleted);

        if (projectId.HasValue)
            query = query.Where(e => e.ProjectId == projectId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Task>> GetAllForProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.ProjectId == projectId);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Task>> GetAllForUserAsync(int userId, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.AppointeeEmployeeId == userId);

        if (projectId.HasValue)
            query = query.Where(e => e.ProjectId == projectId.Value);

        return await query.ToListAsync(cancellationToken);
    }
}
