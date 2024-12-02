using EmployeeAdministration.Application.Common.DTOs;

namespace EmployeeAdministration.Application.Abstractions.Services;

public interface IAuthService
{
    Task<bool> IsUserAuthorizedAsync(
        int userId,
        string[] allowedRoles,
        CancellationToken cancellationToken = default);

    Task<Tokens> RefreshTokensAsync(Tokens request, CancellationToken cancellationToken = default);
}