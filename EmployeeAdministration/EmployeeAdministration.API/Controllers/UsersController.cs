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
    
    [Authorize(Roles = nameof(Roles.Administrator))]
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var newUser = await _servicesManager.UsersService
                                            .CreateUserAsync(request, cancellationToken);

        return Created(string.Empty, newUser); 
    }

    [Authorize(Roles = nameof(Roles.Administrator))]
    [HttpGet]
    public async Task<IActionResult> GetAllUsersAsync(
        CancellationToken cancellationToken,
        Roles? filterByRole = null, 
        bool includeDeletedUsers = false)
    {
        var users = await _servicesManager.UsersService
                                          .GetAllAsync(filterByRole, includeDeletedUsers, cancellationToken);

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
