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
//[ApiExplorerSettings(GroupName = "Tasks")]
public class TasksController : BaseController
{
    public TasksController(IServicesManager servicesManager) : base(servicesManager)
    {
    }

    [HttpGet("{id:int:min(1)}")]
    [SwaggerOperation("Retrieve task instance")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Application.Common.DTOs.Task))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Task could not be found", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Requester is not an administrator or not the employee assigned the task", typeof(ProblemDetails))]
    public async Task<IActionResult> GetTaskByIdAsync(int id, CancellationToken cancellationToken)
    {
        var task = await _servicesManager.TasksService
                                         .GetByIdAsync(GetRequesterId(HttpContext), id, cancellationToken);

        return Ok(task);
    }

    [HttpPatch("{id:int:min(1)}")]
    [SwaggerOperation("Update a task's properties")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Application.Common.DTOs.Task))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Task could not be found", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Requester is not an administrator or not the employee assigned the task", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid values", typeof(ValidationProblemDetails))]
    public async Task<IActionResult> UpdateTaskAsync(int id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var updatedTask = await _servicesManager.TasksService
                                                .UpdateAsync(GetRequesterId(HttpContext), id, request, cancellationToken);

        return Ok(updatedTask);
    }

    [Authorize(Roles = nameof(Roles.Administrator))]
    [HttpDelete("{id:int:min(1)}")]
    [SwaggerOperation("Delete a task")]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Task could not be found", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized access")]
    public async Task<IActionResult> DeleteTaskAsync(int id, CancellationToken cancellationToken)
    {
        await _servicesManager.TasksService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
