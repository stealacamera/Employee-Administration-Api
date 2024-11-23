using EmployeeAdministration.Application.Abstractions.Interfaces;

namespace EmployeeAdministration.Application.Abstractions;

public interface IServicesManager
{
    IUsersService UsersService { get; }
}
