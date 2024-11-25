using EmployeeAdministration.Domain.Entities;

namespace EmployeeAdministration.Application.Abstractions.Repositories;

public interface IProjectMembersRepository : IBaseRepository<ProjectMember>
{
    Task<bool> IsUserMemberAsync(int userId, int projectId, CancellationToken cancellationToken = default);

    Task<ProjectMember?> GetByIdsAsync(int userId, int projectId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProjectMember>> GetAllForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectMember>> GetAllForProjectAsync(int projectId, CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task DeleteAllForProjectAsync(int projectId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAllForUserAsync(int userId, CancellationToken cancellationToken = default);
}
