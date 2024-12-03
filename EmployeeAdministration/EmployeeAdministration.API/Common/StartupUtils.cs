using System.Globalization;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.API.Common;

internal static class StartupUtils
{
    private static string[] _apiGroups = ["Identity", "Projects", "Tasks", "Users"];

    public static void RegisterUtils(this IServiceCollection services)
    {
        services.AddSwaggerGen(RegisterSwagger);

        services.AddTransient<ExceptionHandlingMiddleware>();
        services.AddSingleton<IAuthorizationHandler, CustomRoleAuthorizationHandler>();

        services.AddRouting(options => options.LowercaseUrls = true);
    }

    public static void RegisterJsonConverters(JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new SmartEnumNameConverter<ProjectStatuses, sbyte>());
    }

    public static async Task SeedAdmin(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var servicesManager = scope.ServiceProvider.GetRequiredService<IServicesManager>();
            var adminEmail = "admin@email.com";

            if (!await servicesManager.UsersService.DoesUserEmailExistAsync(adminEmail, includeDeletedEntities: true))
            {
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

        // Register groups
        foreach (var group in _apiGroups)
            options.SwaggerDoc(group, new OpenApiInfo { Title = group, Version = "v1" });

        options.MapType<ProjectStatuses>(() =>
            new OpenApiSchema
            {
                Type = "string",
                Enum = ProjectStatuses.List.Select(e => new OpenApiString(e.Name)).ToArray()
            });

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

    internal static void RegisterSwaggerGroups(SwaggerUIOptions options)
    {
        foreach (var group in _apiGroups)
            options.SwaggerEndpoint($"/swagger/{group}/swagger.json", $"{group} API");
    }
}
