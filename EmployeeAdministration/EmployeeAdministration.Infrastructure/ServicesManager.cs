using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Interfaces;
using EmployeeAdministration.Application.Abstractions.Services;
using EmployeeAdministration.Application.Abstractions.Services.Utils;
using EmployeeAdministration.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAdministration.Infrastructure;

internal class ServicesManager : IServicesManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkUnit _workUnit;

    public ServicesManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _workUnit = _serviceProvider.GetRequiredService<IWorkUnit>();
    }

    public IUsersService _usersService = null!;
    public IUsersService UsersService
    {
        get
        {
            _usersService ??= new UsersService(
                                _workUnit, 
                                _serviceProvider.GetRequiredService<IJwtProvider>(), 
                                _serviceProvider.GetRequiredService<IImagesStorageService>());

            return _usersService;
        }
    }

    private IProjectsService _projectsService = null!;
    public IProjectsService ProjectsService
    {
        get
        {
            _projectsService ??= new ProjectsService(_workUnit);
            return _projectsService;
        }
    }

    private IProjectMembersService _projectMembersService = null!;
    public IProjectMembersService ProjectMembersService
    {
        get
        {
            _projectMembersService ??= new ProjectMembersService(_workUnit);
            return _projectMembersService;
        }
    }

    private ITasksService _tasksService = null!;
    public ITasksService TasksService
    {
        get
        {
            _tasksService ??= new TasksService(_workUnit, _serviceProvider.GetRequiredService<IEventBus>());
            return _tasksService;
        }
    }

    private IAuthService _authService = null!;
    public IAuthService AuthService
    {
        get
        {
            _authService ??= new AuthService(_workUnit, _serviceProvider.GetRequiredService<IJwtProvider>());
            return _authService;
        }
    }
}
