using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EmployeeAdministration.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
//[ApiExplorerSettings(GroupName = "Identity")]
public class IdentityController : BaseController
{
    public IdentityController(IServicesManager servicesManager) : base(servicesManager) { }

    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation("Log in as user")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(LoggedInUser))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid values, or wrong credentials")]
    public async Task<IActionResult> LoginAsync(VerifyCredentialsRequest request, CancellationToken cancellationToken)
    {
        var loggedinUser = await _servicesManager.UsersService
                                                  .VerifyCredentialsAsync(request, cancellationToken);

        return loggedinUser != null ?
               Ok(loggedinUser) :
               BadRequest(new ProblemDetails
               {
                   Title = "Invalid credentials",
                   Detail = "Incorrect email and/or password",
                   Status = StatusCodes.Status400BadRequest
               });
    }

    [AllowAnonymous]
    [HttpPost("tokens")]
    [SwaggerOperation("Refresh JWT and refresh tokens")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Tokens))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Expired tokens, requester needs to log in again", type: typeof(ProblemDetails))]
    public async Task<IActionResult> RefreshTokensAsync(Tokens expiredTokens, CancellationToken cancellationToken)
    {
        var refreshedTokens = await _servicesManager.AuthService
                                                    .RefreshTokensAsync(expiredTokens, cancellationToken);
        
        return Ok(refreshedTokens);
    }

    [HttpPut("password")]
    [SwaggerOperation("Update user's password")]
    [SwaggerResponse(StatusCodes.Status200OK, "Password updated successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized access", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid values, or incorrect password", typeof(ProblemDetails))]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest request, CancellationToken cancellationToken)
    {
        await _servicesManager.UsersService
                              .UpdatePasswordAsync(GetRequesterId(HttpContext), request, cancellationToken);

        return Ok();
    }

    [Authorize(Roles = nameof(Roles.Employee))]
    [HttpGet("profile")]
    [SwaggerOperation("Retrieve user information", "Retrieve user's details, projects and their tasks in each project")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(UserProfile))]
    public async Task<IActionResult> GetUserProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await _servicesManager.UsersService
                                            .GetProfileByIdAsync(GetRequesterId(HttpContext), cancellationToken);

        return Ok(profile);
    }
}
