using EmployeeAdministration.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class TasksRepository : Repository<Domain.Entities.Task>, ITasksRepository
{
    public TasksRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<bool> DoesUserHaveOpenTasksAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => e.AppointeeEmployeeId == userId && !e.IsCompleted);
        return await query.AnyAsync(cancellationToken);
    }
}
