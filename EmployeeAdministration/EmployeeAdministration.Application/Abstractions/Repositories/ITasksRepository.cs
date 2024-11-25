namespace EmployeeAdministration.Application.Abstractions.Repositories;

public interface ITasksRepository : IRepository<Domain.Entities.Task>
{
    Task<IEnumerable<Domain.Entities.Task>> GetAllForProjectAsync(int projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Task>> GetAllForUserAsync(
        int userId, 
        int? projectId = null, 
        CancellationToken cancellationToken = default);

    Task DeleteAllForProjectAsync(int projectId, CancellationToken cancellationToken = default);
    
    Task<bool> DoesUserHaveOpenTasksAsync(int userId, int? projectId = null, CancellationToken cancellationToken = default);
    Task<bool> DoesProjectHaveOpenTasksAsync(int projectId, CancellationToken cancellationToken = default);
}
