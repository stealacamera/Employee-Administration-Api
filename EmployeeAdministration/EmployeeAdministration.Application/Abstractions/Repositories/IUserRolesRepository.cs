using EmployeeAdministration.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Application.Abstractions.Repositories;

public interface IUserRolesRepository
{
    Task AddToRoleAsync(int userId, Roles role, CancellationToken cancellationToken = default);
    Task<bool> IsUserInRoleAsync(int userId, Roles role, CancellationToken cancellationToken = default);
    Task<Roles> GetUserRoleAsync(int userId, CancellationToken cancellationToken = default);
}
