using EmployeeAdministration.Domain.Entities;

namespace EmployeeAdministration.Application.Abstractions.Repositories;

public interface IProjectsRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetAllByIdsAsync(int[] ids, CancellationToken cancellationToken = default);
}
