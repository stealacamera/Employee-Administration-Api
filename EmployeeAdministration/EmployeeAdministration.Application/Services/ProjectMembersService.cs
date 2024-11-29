using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;

namespace EmployeeAdministration.Application.Services;

internal class ProjectMembersService : BaseService, IProjectMembersService
{
    public ProjectMembersService(IWorkUnit workUnit) : base(workUnit) { }

    public async Task<ProjectMember> AddEmployeeToProjectAsync(int employeeId, int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _workUnit.ProjectsRepository.GetByIdAsync(projectId, cancellationToken);
        var user = await _workUnit.UsersRepository.GetByIdAsync(employeeId, cancellationToken: cancellationToken);

        if (project == null)
            throw new EntityNotFoundException(nameof(Project));

        // Only permit users that are employees and not apart of the project
        if (user == null)
            throw new EntityNotFoundException(nameof(User));
        else if (!await _workUnit.UsersRepository.IsUserInRoleAsync(user, Domain.Enums.Roles.Employee, cancellationToken))
            throw new NonEmployeeUserException();
        else if (await _workUnit.ProjectMembersRepository.IsUserMemberAsync(employeeId, projectId, cancellationToken))
            throw new ExistingProjectMemberException();

        await _workUnit.ProjectMembersRepository
                       .AddAsync(
                            new Domain.Entities.ProjectMember
                            {
                                CreatedAt = DateTime.UtcNow,
                                EmployeeId = employeeId,
                                ProjectId = projectId
                            }, cancellationToken);

        return new ProjectMember(
            new BriefProject(project.Id, project.Name, project.Description),
            new BriefUser(user.Id, user.Email, user.FirstName, user.Surname, user.DeletedAt));
    }

    public async System.Threading.Tasks.Task RemoveEmployeeFromProjectAsync(int employeeId, int projectId, CancellationToken cancellationToken = default)
    {
        // Check if the employee & the project exist, and if the employee is part of the project
        var project = await _workUnit.ProjectsRepository.GetByIdAsync(projectId, cancellationToken);
        var user = await _workUnit.UsersRepository.GetByIdAsync(employeeId, cancellationToken: cancellationToken);

        if (project == null)
            throw new EntityNotFoundException(nameof(Project));
        else if (user == null)
            throw new EntityNotFoundException(nameof(User));

        var member = await _workUnit.ProjectMembersRepository
                                    .GetByIdsAsync(employeeId, projectId, cancellationToken);

        if (member == null)
            throw new NotAProjectMemberException();

        // Don't complete if the member has open tasks for the project
        if (await _workUnit.TasksRepository.DoesUserHaveOpenTasksAsync(employeeId, projectId, cancellationToken))
            throw new UncompletedTasksAssignedToEntityException(nameof(User));

        // Remove member
        _workUnit.ProjectMembersRepository.Delete(member);
        await _workUnit.SaveChangesAsync();
    }
}
