using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAdministration.Infrastructure;

public static class Startup
{
    public static void RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Database")));

        services.AddIdentityCore<User>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;

                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";

                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<AppDbContext>();

        // Should both jwt + cookies be used? or exclusively one?
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.MapInboundClaims = false;
                });

        services.AddAuthorization();
        
        services.ConfigureOptions<JwtOptionsSetup>();

        services.AddScoped<IJwtProvider, JwtProvider>();
        services.ConfigureOptions<JwtBearerOptionsSetup>();
        services.AddScoped<IWorkUnit, WorkUnit>();
    }
}
