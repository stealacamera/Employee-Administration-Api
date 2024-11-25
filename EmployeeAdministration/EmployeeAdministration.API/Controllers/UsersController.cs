using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdministration.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    public UsersController(IServicesManager servicesManager) : base(servicesManager) { }

    // TODO add jwt
    // TODO make password encrypted
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(VerifyCredentialsRequest request, CancellationToken cancellationToken)
    {
        var loggedinUser =  await _servicesManager.UsersService
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

    [Authorize(Roles = nameof(Roles.Administrator))]
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var newUser = await _servicesManager.UsersService
                                            .CreateUserAsync(request, cancellationToken);

        return Created(string.Empty, newUser); 
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await _servicesManager.UsersService
                                            .GetProfileByIdAsync(GetRequesterId(HttpContext), cancellationToken);
        
        return Ok(profile);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _servicesManager.UsersService
                                          .GetAllAsync(GetRequesterId(HttpContext), cancellationToken);

        return Ok(users);
    }

    [Authorize(Roles = nameof(Roles.Administrator))]
    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        await _servicesManager.UsersService
                              .DeleteUserAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:int:min(1)}")]
    public async Task<IActionResult> UpdateUserAsync(
        int id,
        [FromBody] UpdateUserRequest request, 
        CancellationToken cancellationToken)
    {
        var updatedUser = await _servicesManager.UsersService
                                                .UpdateUserAsync(
                                                    GetRequesterId(HttpContext), 
                                                    id, 
                                                    request, 
                                                    cancellationToken);

        return Ok(updatedUser);
    }
}
