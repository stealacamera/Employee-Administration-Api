using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmployeeAdministration.API.Common;

internal static class StartupUtils
{
    public static void RegisterUtils(this IServiceCollection services)
    {
        services.AddSwaggerGen(RegisterSwagger);

        services.AddTransient<ExceptionHandlingMiddleware>();
        services.AddSingleton<IAuthorizationHandler, CustomRoleAuthorizationHandler>();
    }

    private static void RegisterSwagger(SwaggerGenOptions options)
    {
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
