using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Application.Abstractions.Repositories;

public interface IUsersRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(
        bool includeDeletedUsers = false,
        Roles? filterByRole = null, 
        CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<bool> VerifyCredentialsAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<bool> IsEmailInUseAsync(string email, bool includeDeletedUsers = false, CancellationToken cancellationToken = default);
    Task<bool> DoesUserExistAsync(int id, bool includeDeletedUsers = false, CancellationToken cancellationToken = default);

    Task<Roles> GetUserRoleAsync(User user, CancellationToken cancellationToken = default);
    Task AddToRoleAsync(User user, Roles role, CancellationToken cancellationToken = default);
    Task<bool> IsUserInRoleAsync(User user, Roles role, CancellationToken cancellationToken = default);

    Task<bool> IsPasswordCorrectAsync(User user, string password, CancellationToken cancellationToken = default);
    Task UpdatePassword(User user, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
}
