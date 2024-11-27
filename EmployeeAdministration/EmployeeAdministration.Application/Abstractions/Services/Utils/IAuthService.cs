namespace EmployeeAdministration.Application.Abstractions.Services.Utils;

public interface IAuthService
{
    Task<bool> IsUserAuthorizedAsync(
        int userId, 
        string[] allowedRoles, 
        CancellationToken cancellationToken = default);
}
