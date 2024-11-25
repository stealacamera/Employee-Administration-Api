using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Application;

public static class Startup
{
    public static void RegisterApplication(this IServiceCollection services)
    {
        services.AddScoped<IServicesManager, ServicesManager>();

        var servicesManager = services.BuildServiceProvider().GetRequiredService<IServicesManager>();
        SeedAdmin(servicesManager).GetAwaiter().GetResult();
    }

    private static async Task SeedAdmin(IServicesManager servicesManager)
    {
        var adminEmail = "admin@email.com";

        if (!await servicesManager.UsersService.DoesUserEmailExistAsync(adminEmail, includeDeletedEntities: true))
        {
            // TODO move details to appsettings?
            var adminRequest = new CreateUserRequest(
                Roles.Administrator, adminEmail, 
                "Admin", "Admin", "base//admin12password");

            await servicesManager.UsersService
                                 .CreateUserAsync(adminRequest);
        }
    }
}
