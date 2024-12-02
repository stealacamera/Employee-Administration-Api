using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Common.Exceptions.General;

namespace EmployeeAdministration.Infrastructure.Services;

internal class AuthService : IAuthService
{
    private readonly IWorkUnit _workUnit;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IWorkUnit workUnit, IJwtProvider jwtProvider)
    {
        _workUnit = workUnit;
        _jwtProvider = jwtProvider;
    }

    public async Task<bool> IsUserAuthorizedAsync(
        int userId,
        string[] allowedRoles,
        CancellationToken cancellationToken = default)
    {
        var user = await _workUnit.UsersRepository
                                  .GetByIdAsync(userId);

        if (user == null)
            return false;

        var userRole = await _workUnit.UserRolesRepository
                                      .GetUserRoleAsync(user.Id, cancellationToken);

        return allowedRoles.Contains(userRole.ToString());
    }

    public async Task<Tokens> RefreshTokensAsync(Tokens request, CancellationToken cancellationToken = default)
    {
        var requester = await ValidateRefreshRequestAsync(request, cancellationToken);

        _jwtProvider.UpdateRefreshToken(requester);
        await _workUnit.SaveChangesAsync();

        return new Tokens(_jwtProvider.GenerateToken(requester.Id, requester.Email), requester.RefreshToken);
    }

    private async Task<Domain.Entities.User> ValidateRefreshRequestAsync(Tokens request, CancellationToken cancellationToken)
    {
        var tokenUserId = _jwtProvider.ExtractIdFromToken(request.JwtToken);

        var requester = await _workUnit.UsersRepository.GetByIdAsync(tokenUserId);

        if (requester == null || requester.RefreshToken != request.RefreshToken)
            throw new UnauthorizedException();
        else if (requester.RefreshTokenExpiry < DateTime.UtcNow)
            throw new ExpiredRefreshTokenException();

        return requester;
    }
}
