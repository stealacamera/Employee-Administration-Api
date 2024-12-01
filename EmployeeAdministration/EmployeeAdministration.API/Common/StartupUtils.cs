using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.API.Common;

internal static class StartupUtils
{
    public static void RegisterUtils(this IServiceCollection services)
    {
        services.AddSwaggerGen(RegisterSwagger);

        services.AddTransient<ExceptionHandlingMiddleware>();
        services.AddSingleton<IAuthorizationHandler, CustomRoleAuthorizationHandler>();
    }

    public static async Task SeedAdmin(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var servicesManager = scope.ServiceProvider.GetRequiredService<IServicesManager>();
            var adminEmail = "admin@email.com";

            if (!await servicesManager.UsersService.DoesUserEmailExistAsync(adminEmail, includeDeletedEntities: true))
            {
                // TODO move details to appsettings?
                var adminRequest = new CreateUserRequest(
                    Roles.Administrator, adminEmail,
                    "Admin", "Admin", "base//admin12password");

                await servicesManager.UsersService.CreateUserAsync(adminRequest);
            }
        }
    }

    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<AppDbContext>();
            
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();
        }
    }

    private static void RegisterSwagger(SwaggerGenOptions options)
    {
        options.EnableAnnotations();

        //options.DocInclusionPredicate((docName, apiDescription) =>
        //{
        //    var groupName = apiDescription.ActionDescriptor.RouteValues["controller"];
        //    return groupName.ToLower() == docName.ToLower();
        //});

        // Add JWT authentication
        options.AddSecurityDefinition(
            JwtBearerDefaults.AuthenticationScheme,
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                Description = "JWT authentication (no need to add 'Bearer' in the beginning)",
            });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme }},
                new string[] {}
            }
        });
    }
}
