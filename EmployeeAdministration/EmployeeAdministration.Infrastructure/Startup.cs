using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services.Utils;
using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Infrastructure.Options.Setups;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Configuration;

namespace EmployeeAdministration.Infrastructure;

public static class Startup
{
    public static void RegisterInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

        builder.Services.AddIdentityCore<User>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;

                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";

                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<AppDbContext>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.MapInboundClaims = false;
                });

        builder.Services.AddAuthorization();
        
        builder.Services.ConfigureOptions<JwtOptionsSetup>();
        builder.Services.ConfigureOptions<CloudinaryOptionsSetup>();
        builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();

        builder.Services.AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration.GetConnectionString("Redis"));

        builder.Services.AddScoped<IJwtProvider, JwtProvider>();
        
        builder.Services.AddScoped<IImagesService, ImagesService>();
        builder.Services.AddScoped<IWorkUnit, WorkUnit>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Register logger
        Log.Logger = new LoggerConfiguration().ReadFrom
                                              .Configuration(builder.Configuration)
                                              .CreateLogger();

        builder.Logging.AddSerilog(Log.Logger);
        builder.Host.UseSerilog();
    }
}
