using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Common.Exceptions.General;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Infrastructure.Services;

internal class ProjectMembersService : BaseService, IProjectMembersService
{
    public ProjectMembersService(IWorkUnit workUnit) : base(workUnit) { }

    public async Task<IList<ProjectMember>> AddEmployeesToProjectAsync(int[] employeeIds, int projectId, CancellationToken cancellationToken = default)
    {
        var projectMembers = new List<ProjectMember>();
        var project = await _workUnit.ProjectsRepository.GetByIdAsync(projectId, cancellationToken);

        if (project == null)
            throw new EntityNotFoundException(nameof(Project));

        await WrapInTransactionAsync(async () =>
        {
            foreach (int employeeId in employeeIds)
            {
                var employee = await _workUnit.UsersRepository.GetByIdAsync(employeeId, cancellationToken: cancellationToken);
                await ValidateEmployeeToAddAsync(employee, projectId, cancellationToken);

                await _workUnit.ProjectMembersRepository
                               .AddAsync(
                                    new Domain.Entities.ProjectMember
                                    {
                                        CreatedAt = DateTime.UtcNow,
                                        EmployeeId = employeeId,
                                        ProjectId = projectId
                                    }, cancellationToken);

                await _workUnit.SaveChangesAsync();

                projectMembers.Add(
                    new ProjectMember(
                        new BriefProject(project.Id, project.Name, project.Description),
                        new BriefUser(employee.Id, employee.Email, employee.FirstName, employee.Surname, employee.DeletedAt)));
            }
        });

        return projectMembers;
    }

    public async Task RemoveEmployeesFromProjectAsync(int[] employeeIds, int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _workUnit.ProjectsRepository.GetByIdAsync(projectId, cancellationToken);

        if (project == null)
            throw new EntityNotFoundException(nameof(Project));

        await WrapInTransactionAsync(async () =>
        {
            foreach (int employeeId in employeeIds)
            {
                var user = await _workUnit.UsersRepository.GetByIdAsync(employeeId, cancellationToken: cancellationToken);
                var membership = await ValidateEmployeeToRemoveAsync(user, projectId, cancellationToken);

                // Remove member
                _workUnit.ProjectMembersRepository.Delete(membership);
                await _workUnit.SaveChangesAsync();
            }
        });
    }

    private async Task<Domain.Entities.ProjectMember> ValidateEmployeeToRemoveAsync(
        Domain.Entities.User? employee, 
        int projectId, 
        CancellationToken cancellationToken)
    {
        if (employee == null)
            throw new EntityNotFoundException(nameof(User));

        var member = await _workUnit.ProjectMembersRepository
                                    .GetByIdsAsync(employee.Id, projectId, cancellationToken);

        if (member == null)
            throw new NotAProjectMemberException();

        // Don't complete if the member has open tasks for the project
        var doesEmployeeHaveOpenTasks = await _workUnit.TasksRepository
                                                       .DoesUserHaveOpenTasksAsync(employee.Id, projectId, cancellationToken);

        if (doesEmployeeHaveOpenTasks)
            throw new UncompletedTasksAssignedToEntityException(nameof(User));

        return member;
    }

    private async Task ValidateEmployeeToAddAsync(
        Domain.Entities.User? employee, 
        int projectId, 
        CancellationToken cancellationToken)
    {
        // Only permit users that are employees and not apart of the project
        if (employee == null)
            throw new EntityNotFoundException(nameof(User));

        var isUserEmployee = await _workUnit.UserRolesRepository
                                            .IsUserInRoleAsync(employee.Id, Domain.Enums.Roles.Employee, cancellationToken);

        if (!isUserEmployee)
            throw new NonEmployeeUserException();

        var isUserExistingMember = await _workUnit.ProjectMembersRepository
                                                  .IsUserMemberAsync(employee.Id, projectId, cancellationToken);
        
        if (isUserExistingMember)
            throw new ExistingProjectMemberException();
    }
}
