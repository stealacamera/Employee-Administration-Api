using EmployeeAdministration.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace EmployeeAdministration.Application.Abstractions;

public interface IWorkUnit
{
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();

    IUsersRepository UsersRepository { get; }
    IUserRolesRepository UserRolesRepository { get; }
    IProjectsRepository ProjectsRepository { get; }
    IProjectMembersRepository ProjectMembersRepository { get; }
    ITasksRepository TasksRepository { get; }
}
