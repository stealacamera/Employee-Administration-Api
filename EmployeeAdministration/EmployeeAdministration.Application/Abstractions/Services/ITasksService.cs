using EmployeeAdministration.Application.Common.DTOs;
using Task = EmployeeAdministration.Application.Common.DTOs.Task;

namespace EmployeeAdministration.Application.Abstractions.Services;

public interface ITasksService
{
    Task<Task> GetByIdAsync(int requesterId, int id, CancellationToken cancellationToken = default); 
    Task<IList<Task>> GetAllForProjectAsync(int requesterId, int projectId, CancellationToken cancellationToken = default);
    
    Task<Task> CreateAsync(
        int requesterId, 
        int projectId,
        CreateTaskRequest request, 
        CancellationToken cancellationToken = default);

    Task<Task> UpdateAsync(
        int requesterId, 
        int id, 
        UpdateTaskRequest request, 
        CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
