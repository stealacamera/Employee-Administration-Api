using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Common.Exceptions.General;
using EmployeeAdministration.Domain.Enums;
using Task = EmployeeAdministration.Application.Common.DTOs.Task;

namespace EmployeeAdministration.Infrastructure.Services;

internal class ProjectsService : BaseService, IProjectsService
{
    public ProjectsService(IWorkUnit workUnit) : base(workUnit) { }

    public async Task<ComprehensiveProject> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        IList<BriefUser> projectMembers = new List<BriefUser>();
        var newProject = new Domain.Entities.Project
        {
            CreatedAt = DateTime.UtcNow,
            Name = request.Name,
            Description = request.Description,
            StatusId = ProjectStatuses.InProgress.Id
        };

        // Create project
        await _workUnit.ProjectsRepository.AddAsync(newProject, cancellationToken);
        await _workUnit.SaveChangesAsync();

        // Create memberships
        if (request.EmployeeIds != null && request.EmployeeIds.Length > 0)
            projectMembers = await CreateProjectMembersAsync(newProject.Id, request.EmployeeIds, cancellationToken);

        return new ComprehensiveProject(
            newProject.Id, newProject.Name, ProjectStatuses.InProgress,
            new List<Task>(), projectMembers,
            newProject.Description);
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _workUnit.ProjectsRepository
                                     .GetByIdAsync(id, cancellationToken);

        if (project == null)
            throw new EntityNotFoundException(nameof(Project));

        // Check if there are unfinished tasks for project
        if (await _workUnit.TasksRepository.DoesProjectHaveOpenTasksAsync(id, cancellationToken))
            throw new UncompletedTasksAssignedToEntityException(nameof(Project));

        await WrapInTransactionAsync(async () =>
        {
            // Delete all tasks
            await _workUnit.TasksRepository.DeleteAllForProjectAsync(id, cancellationToken);
            await _workUnit.SaveChangesAsync();

            // Delete all memberships
            await _workUnit.ProjectMembersRepository.DeleteAllForProjectAsync(id, cancellationToken);
            await _workUnit.SaveChangesAsync();

            // Delete project
            _workUnit.ProjectsRepository.Delete(project);
            await _workUnit.SaveChangesAsync();
        });
    }

    public async Task<ComprehensiveProject> GetByIdAsync(int id, int requesterId, CancellationToken cancellationToken = default)
    {
        // Retrieve project
        var project = await _workUnit.ProjectsRepository
                                     .GetByIdAsync(id, cancellationToken);

        if(project == null)
            throw new EntityNotFoundException(nameof(Project));
        
        await ValidateRequesterIsAdminOrProejctMember(requesterId, project.Id, cancellationToken);

        return new ComprehensiveProject(
            project.Id, project.Name, ProjectStatuses.FromId(project.StatusId),
            await GetProjectTasksAsync(id, cancellationToken), 
            await GetProjectMembersAsync(id, cancellationToken), 
            project.Description);
    }

    public async Task<BriefProject> UpdateAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Status == null && request.Name == null && request.Description == null)
            ValidationException.GenerateExceptionForEmptyRequest();

        var project = await _workUnit.ProjectsRepository
                                     .GetByIdAsync(id, cancellationToken);

        if(project == null)
            throw new EntityNotFoundException(nameof(Project));

        // Update attributes
        if (request.Name != null)
            project.Name = request.Name;
        if (request.Description != null)
            project.Description = request.Description;
        if (request.Status != null)
            project.StatusId = request.Status.Id;

        project.UpdatedAt = DateTime.UtcNow;
        await _workUnit.SaveChangesAsync();

        return new BriefProject(id, project.Name, project.Description);
    }


    // Helper functions
    private async System.Threading.Tasks.Task ValidateRequesterIsAdminOrProejctMember(
        int requesterId,
        int projectId,
        CancellationToken cancellationToken)
    {
        var requester = await _workUnit.UsersRepository
                                       .GetByIdAsync(requesterId, cancellationToken: cancellationToken);

        if (requester == null)
            throw new UnauthorizedException();

        var isRequesterAdmin = await _workUnit.UserRolesRepository
                                              .IsUserInRoleAsync(requester.Id, Roles.Administrator, cancellationToken);

        if (!isRequesterAdmin)
        {
            if (!await _workUnit.ProjectMembersRepository.IsUserMemberAsync(requesterId, projectId, cancellationToken))
                throw new UnauthorizedException();
        }
    }

    private async Task<IList<BriefUser>> CreateProjectMembersAsync(int projectId, int[] userIds, CancellationToken cancellationToken)
    {
        var projectMembers = new List<BriefUser>();

        await WrapInTransactionAsync(async () =>
        {
            foreach (var userId in userIds)
            {
                // Skip user if they don't exist or they're not an employee
                var user = await _workUnit.UsersRepository
                                          .GetByIdAsync(userId, cancellationToken: cancellationToken);

                if (user == null)
                    continue;

                var userRole = await _workUnit.UserRolesRepository
                                              .GetUserRoleAsync(user.Id, cancellationToken);

                if (userRole != Roles.Employee)
                    continue;

                await _workUnit.ProjectMembersRepository
                               .AddAsync(
                                    new Domain.Entities.ProjectMember
                                    {
                                        CreatedAt = DateTime.UtcNow,
                                        EmployeeId = userId,
                                        ProjectId = projectId
                                    },
                                    cancellationToken);

                await _workUnit.SaveChangesAsync();

                projectMembers.Add(new BriefUser(
                    user.Id, user.Email, user.FirstName,
                    user.Surname, user.DeletedAt));
            }
        });

        return projectMembers;
    }

    private async Task<IList<Task>> GetProjectTasksAsync(int projectId, CancellationToken cancellationToken)
    {
        List<Task> tasks = new();
        var taskModels = await _workUnit.TasksRepository
                                        .GetAllForProjectAsync(projectId, cancellationToken);

        foreach(var task in taskModels)
        {
            var appointee = (await _workUnit.UsersRepository
                                            .GetByIdAsync(task.AppointeeEmployeeId,excludeDeletedUser: false, cancellationToken))!;

            var appointeeModel = new BriefUser(
                appointee.Id, appointee.Email,
                appointee.FirstName, appointee.Surname,
                appointee.DeletedAt);

            BriefUser? appointerModel = null;

            // Don't get task-appointer if the task is self-assigned
            if (task.AppointeeEmployeeId != task.AppointerUserId)
            {
                var appointer = (await _workUnit.UsersRepository
                                                .GetByIdAsync(task.AppointeeEmployeeId, excludeDeletedUser: false, cancellationToken))!;

                appointerModel = new BriefUser(
                    appointee.Id, appointee.Email,
                    appointee.FirstName, appointee.Surname,
                    appointee.DeletedAt);
            }

            tasks.Add(new Task(
                task.Id, appointerModel, appointeeModel, 
                task.Name, task.IsCompleted, 
                task.CreatedAt, task.Description));
        }

        return tasks;
    }

    private async Task<IList<BriefUser>> GetProjectMembersAsync(int projectId, CancellationToken cancellationToken)
    {
        List<BriefUser> members = new();
        var memberships = await _workUnit.ProjectMembersRepository
                                         .GetAllForProjectAsync(projectId, cancellationToken);

        foreach (var membership in memberships)
        {
            var employeeMember = (await _workUnit.UsersRepository
                                         .GetByIdAsync(membership.EmployeeId, cancellationToken: cancellationToken))!;

            members.Add(new BriefUser(
                employeeMember.Id, employeeMember.Email, 
                employeeMember.FirstName, employeeMember.Surname, 
                employeeMember.DeletedAt));
        }

        return members;
    }
}
