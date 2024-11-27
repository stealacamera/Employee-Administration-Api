using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services.Utils;

namespace EmployeeAdministration.Infrastructure;

internal class AuthService : IAuthService
{
    private readonly IWorkUnit _workUnit;

    public AuthService(IWorkUnit workUnit)
        => _workUnit = workUnit;

    public async Task<bool> IsUserAuthorizedAsync(
        int userId, 
        string[] allowedRoles, 
        CancellationToken cancellationToken = default)
    {
        var user = await _workUnit.UsersRepository
                                  .GetByIdAsync(userId);

        if (user == null)
            return false;

        var userRole = await _workUnit.UsersRepository
                                      .GetUserRoleAsync(user, cancellationToken);

        return allowedRoles.Contains(userRole.ToString());
    }
}
