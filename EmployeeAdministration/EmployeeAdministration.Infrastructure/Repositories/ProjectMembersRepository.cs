using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class ProjectMembersRepository : BaseRepository<ProjectMember>, IProjectMembersRepository
{
    public ProjectMembersRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task DeleteAllMembershipsForUserAsync(int userId, CancellationToken cancellationToken = default)
        => await _set.Where(e => e.EmployeeId == userId)
                     .ExecuteDeleteAsync(cancellationToken);
}
