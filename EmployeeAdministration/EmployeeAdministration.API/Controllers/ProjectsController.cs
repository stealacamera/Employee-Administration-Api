using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdministration.API.Controllers;

[Authorize(Roles = nameof(Roles.Administrator))]
[Route("api/[controller]")]
[ApiController]
public class ProjectsController : BaseController
{
    public ProjectsController(IServicesManager servicesManager) : base(servicesManager) { }

    [Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var project = await _servicesManager.ProjectsService
                                            .GetByIdAsync(id, GetRequesterId(HttpContext), cancellationToken);

        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var newProject = await _servicesManager.ProjectsService
                                               .CreateAsync(request, cancellationToken);

        return Created(string.Empty, newProject);
    }

    [HttpPatch("{id:int:min(1)}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _servicesManager.ProjectsService
                                            .UpdateAsync(id, request, cancellationToken);
        
        return Ok(project);
    }

    
    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _servicesManager.ProjectsService
                              .DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    // Project members

    [HttpPost("{projectId:int:min(1)}/members")]
    public async Task<IActionResult> AddEmployeeToProjectAsync(int projectId, int employeeId, CancellationToken cancellationToken)
    {
        var member = await _servicesManager.ProjectMembersService
                                           .AddEmployeeToProjectAsync(employeeId, projectId, cancellationToken);

        return Created(string.Empty, member);
    }

    [HttpDelete("{projectId:int:min(1)}/members/{userId:int:min(1)}")]
    public async Task<IActionResult> RemoveEmployeeFromProjectAsync(int projectId, int userId, CancellationToken cancellationToken)
    {
        await _servicesManager.ProjectMembersService
                              .RemoveEmployeeFromProjectAsync(userId, projectId, cancellationToken);

        return NoContent();
    }

    // Project tasks

    [HttpGet("{id:int:min(1)}/tasks")]
    public async Task<IActionResult> GetTasksForProjectAsync(int id, CancellationToken cancellationToken)
    {
        var tasks = await _servicesManager.TasksService
                                          .GetAllForProjectAsync(GetRequesterId(HttpContext), id, cancellationToken);

        return Ok(tasks);
    }

    [HttpPost("{id:int:min(1)}/tasks")]
    public async Task<IActionResult> CreateTaskForProjectAsync(int id, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var newTask = await _servicesManager.TasksService
                                            .CreateAsync(GetRequesterId(HttpContext), id, request, cancellationToken);

        return Created(string.Empty, newTask);
    }
}
