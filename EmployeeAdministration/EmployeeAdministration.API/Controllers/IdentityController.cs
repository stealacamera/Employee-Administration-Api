using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EmployeeAdministration.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[ApiExplorerSettings(GroupName = "Identity")]
public class IdentityController : BaseController
{
    public IdentityController(IServicesManager servicesManager) : base(servicesManager) { }

    // TODO make password encrypted
    // TODO add refresh tokens endpoint
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

    [Authorize]
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
    [SwaggerOperation(
        "Retrieve information pertaining to the user", 
        "Retrieve user's current information, the projects they're in and their tasks in each project")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(UserProfile))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized access", type: typeof(ProblemDetails))]
    public async Task<IActionResult> GetUserProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await _servicesManager.UsersService
                                            .GetProfileByIdAsync(GetRequesterId(HttpContext), cancellationToken);

        return Ok(profile);
    }
}
