using EmployeeAdministration.Application.Common.DTOs;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Application.Abstractions.Services;

public interface IProjectMembersService
{
    Task<ProjectMember> AddEmployeeToProjectAsync(int employeeId, int projectId, CancellationToken cancellationToken = default);
    Task RemoveEmployeeFromProjectAsync(int employeeId, int projectId, CancellationToken cancellationToken = default);
}
