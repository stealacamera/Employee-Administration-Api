using EmployeeAdministration.Application.Common.DTOs;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Application.Abstractions.Services;

public interface IProjectMembersService
{
    Task<IList<ProjectMember>> AddEmployeesToProjectAsync(int[] employeeIds, int projectId, CancellationToken cancellationToken = default);
    Task RemoveEmployeesFromProjectAsync(int[] employeeIds, int projectId, CancellationToken cancellationToken = default);
}
