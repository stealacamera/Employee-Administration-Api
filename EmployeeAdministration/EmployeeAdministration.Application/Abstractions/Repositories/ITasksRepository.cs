namespace EmployeeAdministration.Application.Abstractions.Repositories;

public interface ITasksRepository : IRepository<Domain.Entities.Task>
{
    Task<bool> DoesUserHaveOpenTasksAsync(int userId, CancellationToken cancellationToken = default);
}
