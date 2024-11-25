using EmployeeAdministration.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace EmployeeAdministration.API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected readonly IServicesManager _servicesManager;

    protected BaseController(IServicesManager servicesManager)
        => _servicesManager = servicesManager;

    protected int GetRequesterId(HttpContext httpContext)
    {
        var id = httpContext.User
                            .Claims
                            .Where(x => x.Type == JwtRegisteredClaimNames.Sub)
                            .FirstOrDefault()?
                            .Value;

        return int.Parse(id!);
    }
}
