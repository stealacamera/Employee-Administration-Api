using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Interfaces;
using EmployeeAdministration.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAdministration.Application;

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
            _usersService ??= new UsersService(_serviceProvider);
            return _usersService;
        }
    }
}
