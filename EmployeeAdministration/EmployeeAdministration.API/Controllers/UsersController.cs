using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EmployeeAdministration.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
//[ApiExplorerSettings(GroupName = "Users")]
public class UsersController : BaseController
{
    public UsersController(IServicesManager servicesManager) : base(servicesManager) { }
    
    [Authorize(Roles = nameof(Roles.Administrator))]
    [HttpPost]
    [SwaggerOperation("Create a new user with the assigned role")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(User))]
    [SwaggerResponse(
        StatusCodes.Status400BadRequest, 
        "The given values are invalid, or the email has already been registered",
        typeof(ValidationProblemDetails))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unathorized access", typeof(ProblemDetails))]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var newUser = await _servicesManager.UsersService
                                            .CreateUserAsync(request, cancellationToken);

        return Created(string.Empty, newUser); 
    }

    [Authorize(Roles = nameof(Roles.Administrator))]
    [HttpGet]
    [SwaggerOperation("Retrieves all users, with filters regarding the role and the deleted status")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IList<User>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unathorized access", typeof(ProblemDetails))]
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
    [SwaggerResponse(StatusCodes.Status204NoContent, "Deletes a user")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unathorized access", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User could not be found", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "User, if they're an employee, has completed tasks still assigned to them", typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        await _servicesManager.UsersService
                              .DeleteUserAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:int:min(1)}")]
    [SwaggerOperation(
        "Update a user's information", 
        "Updating a user's information is permitted to administrators, and to employees who are updating their own information")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(User))]
    [SwaggerResponse(
        StatusCodes.Status401Unauthorized, 
        "Unauthorized access (requester either isn't an administrator, or they're an employee trying to update someone other than themselves)", 
        typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User specified could not be found", typeof(ProblemDetails))]
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
