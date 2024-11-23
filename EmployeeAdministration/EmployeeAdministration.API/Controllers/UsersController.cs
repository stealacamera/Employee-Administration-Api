using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdministration.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IServicesManager _servicesManager;

    public UsersController(IServicesManager servicesManager)
        => _servicesManager = servicesManager;

    // TODO add jwt
    public OkResult Login()
    {
        return Ok();
    }

    [Authorize(Roles = nameof(Roles.Administrator))]
    public CreatedResult CreateUser()
    {
        return Created();
    }

    [Authorize(Roles = nameof(Roles.Administrator))]
    public NoContentResult DeleteUser()
    {
        return NoContent();
    }

    public OkResult UpdateUser()
    {
        return Ok();
    }
}
