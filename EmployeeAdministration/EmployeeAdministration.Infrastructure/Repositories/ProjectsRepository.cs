using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class ProjectsRepository : Repository<Project>, IProjectsRepository
{
    public ProjectsRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Project>> GetAllByIdsAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        var query = _untrackedSet.Where(e => ids.Contains(e.Id));
        return await query.ToListAsync(cancellationToken);
    }
}
