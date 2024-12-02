using EmployeeAdministration.Application.Abstractions.Interfaces;
using EmployeeAdministration.Application.Abstractions.Services;

namespace EmployeeAdministration.Application.Abstractions;

public interface IServicesManager
{
    IUsersService UsersService { get; }
    IAuthService AuthService { get; }
    IProjectsService ProjectsService { get; }
    IProjectMembersService ProjectMembersService { get; }
    ITasksService TasksService { get; }
}
