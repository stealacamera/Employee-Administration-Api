using EmployeeAdministration.Application.Common.DTOs;

namespace EmployeeAdministration.Application.Abstractions.Interfaces;

public interface IUsersService
{
    Task<bool> VerifyCredentialsAsync(VerifyCredentialsRequest request, CancellationToken cancellationToken = default);

    Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
}
