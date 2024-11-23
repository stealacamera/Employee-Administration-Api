using EmployeeAdministration.Domain.Entities;

namespace EmployeeAdministration.Application.Abstractions.Repositories;

public interface IProjectMembersRepository : IBaseRepository<ProjectMember>
{
    System.Threading.Tasks.Task DeleteAllMembershipsForUserAsync(int userId, CancellationToken cancellationToken = default);
}
