using EmployeeAdministration.Application.Abstractions.Services.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.IdentityModel.Tokens.Jwt;

namespace EmployeeAdministration.API.Common;

internal class CustomRoleAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CustomRoleAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
        => _serviceScopeFactory = serviceScopeFactory;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
    {
        string? userId = context.User.Claims
                                     .Where(x => x.Type == JwtRegisteredClaimNames.Sub)
                                     .FirstOrDefault()?
                                     .Value;

        if (int.TryParse(userId, out int parsedUserId))
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();

            var authService = scope.ServiceProvider
                                   .GetRequiredService<IAuthService>();

            if (await authService.IsUserAuthorizedAsync(parsedUserId, requirement.AllowedRoles.ToArray()))
            {
                context.Succeed(requirement);
                return;
            }
        }

        context.Fail();
    }
}
