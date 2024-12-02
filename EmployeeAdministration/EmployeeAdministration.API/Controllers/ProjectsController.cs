using System.ComponentModel.DataAnnotations;
using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Validation;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EmployeeAdministration.API.Controllers;

[Authorize(Roles = nameof(Roles.Administrator))]
[Route("api/[controller]")]
[ApiController]
//[ApiExplorerSettings(GroupName = "projects")]
public class ProjectsController : BaseController
{
    public ProjectsController(IServicesManager servicesManager) : base(servicesManager) { }

    [Authorize]
    [HttpGet("{id:int:min(1)}")]
    [SwaggerOperation("Get project instance", "Projects can be accessed by administrators, or employees that are project members")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(ComprehensiveProject))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project could not be found")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Requester could not be authorized, or they're not a part of the project")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var project = await _servicesManager.ProjectsService
                                            .GetByIdAsync(id, GetRequesterId(HttpContext), cancellationToken);

        return Ok(project);
    }

    [HttpPost]
    [SwaggerOperation(
        "Create a new project (optionally, with members)", 
        "When creating a project with members, if issues are found with any particular member, " +
        "no errors will be thrown, instead they won't be added to the project")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(ComprehensiveProject))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized access")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid values", typeof(ComprehensiveProject))]
    public async Task<IActionResult> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var newProject = await _servicesManager.ProjectsService
                                               .CreateAsync(request, cancellationToken);

        return Created(string.Empty, newProject);
    }

    [HttpPatch("{id:int:min(1)}")]
    [SwaggerOperation("Update a project's properties")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(BriefProject))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized access", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project could not be found", typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _servicesManager.ProjectsService
                                            .UpdateAsync(id, request, cancellationToken);
        
        return Ok(project);
    }

    
    [HttpDelete("{id:int:min(1)}")]
    [SwaggerOperation("Delete a project")]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized access", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Project has uncompleted tasks", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, type: typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _servicesManager.ProjectsService
                              .DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    // Project members

    [HttpPost("{projectId:int:min(1)}/members")]
    [SwaggerOperation("Add employee(s) to a project")]
    [SwaggerResponse(StatusCodes.Status201Created, type: typeof(ProjectMember))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized access", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project and/or employee could not be found", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "User(s) to be added is not an employee, or they're already a project member", typeof(ProblemDetails))]
    public async Task<IActionResult> AddEmployeeToProjectAsync(
        int projectId, 
        [FromBody, MaxLength(ValidationUtils.MaxEmployeesPerTransaction)] int[] employeeIds, 
        CancellationToken cancellationToken)
    {
        var member = await _servicesManager.ProjectMembersService
                                           .AddEmployeesToProjectAsync(employeeIds, projectId, cancellationToken);

        return Created(string.Empty, member);
    }

    [HttpDelete("{projectId:int:min(1)}/members")]
    [SwaggerOperation("Remove employee(s) from project")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Project member removed succesfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project and/or member could not be found", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "User(s) to be removed is not a member or they have open tasks assigned to them", typeof(ProblemDetails))]
    public async Task<IActionResult> RemoveEmployeeFromProjectAsync(
        int projectId, 
        [FromBody, MaxLength(ValidationUtils.MaxEmployeesPerTransaction)] int[] employeeIds, 
        CancellationToken cancellationToken)
    {
        await _servicesManager.ProjectMembersService
                              .RemoveEmployeesFromProjectAsync(employeeIds, projectId, cancellationToken);

        return NoContent();
    }

    // Project tasks

    [HttpGet("{id:int:min(1)}/tasks")]
    [SwaggerOperation("Retrieve all tasks for project")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(IList<Application.Common.DTOs.Task>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Requester is an employee that is not a project member", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project could not be found", typeof(ProblemDetails))]
    public async Task<IActionResult> GetTasksForProjectAsync(int id, CancellationToken cancellationToken)
    {
        var tasks = await _servicesManager.TasksService
                                          .GetAllForProjectAsync(GetRequesterId(HttpContext), id, cancellationToken);

        return Ok(tasks);
    }

    [HttpPost("{id:int:min(1)}/tasks")]
    [SwaggerOperation("Create a task for a particular project")]
    [SwaggerResponse(StatusCodes.Status201Created, "Task created successfully", typeof(Application.Common.DTOs.Task))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project and/or appointee employee could not be found", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Requester is an employee that's not a project member", typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Appointee user is not an employee or is not a project member", typeof(ProblemDetails))]
    public async Task<IActionResult> CreateTaskForProjectAsync(int id, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var newTask = await _servicesManager.TasksService
                                            .CreateAsync(GetRequesterId(HttpContext), id, request, cancellationToken);

        return Created(string.Empty, newTask);
    }
}
