using EmployeeAdministration.Application.Common.DTOs;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Application.Abstractions.Services;

public interface IProjectsService
{
    Task<ComprehensiveProject> GetByIdAsync(int id, int requesterId, CancellationToken cancellationToken = default);

    Task<ComprehensiveProject> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<BriefProject> UpdateAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
