using EmployeeAdministration.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAdministration.Application;

public static class Startup
{
    public static void RegisterApplication(this IServiceCollection services)
    {
        services.AddScoped<IServicesManager, ServicesManager>();
    }
}
