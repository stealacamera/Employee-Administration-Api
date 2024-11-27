using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdministration.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : BaseController
{
    public IdentityController(IServicesManager servicesManager) : base(servicesManager) { }

    // TODO make password encrypted
    [HttpPost("login")]
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

    [Authorize(Roles = nameof(Roles.Employee))]
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await _servicesManager.UsersService
                                            .GetProfileByIdAsync(GetRequesterId(HttpContext), cancellationToken);

        return Ok(profile);
    }
}
