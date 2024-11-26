using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdministration.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TasksController : BaseController
{
    public TasksController(IServicesManager servicesManager) : base(servicesManager)
    {
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetTaskByIdAsync(int id, CancellationToken cancellationToken)
    {
        var task = await _servicesManager.TasksService
                                         .GetByIdAsync(GetRequesterId(HttpContext), id, cancellationToken);

        return Ok(task);
    }

    [HttpPatch("{id:int:min(1)}")]
    public async Task<IActionResult> UpdateTaskAsync(int id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var updatedTask = await _servicesManager.TasksService
                                                .UpdateAsync(GetRequesterId(HttpContext), id, request, cancellationToken);

        return Ok(updatedTask);
    }

    [Authorize(Roles = nameof(Roles.Administrator))]
    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> DeleteTaskAsync(int id, CancellationToken cancellationToken)
    {
        await _servicesManager.TasksService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
