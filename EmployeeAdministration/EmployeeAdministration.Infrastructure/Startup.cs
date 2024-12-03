using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services;
using EmployeeAdministration.Application.Abstractions.Services.Utils;
using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Infrastructure.Common;
using EmployeeAdministration.Infrastructure.EventHandler.TaskCreated;
using EmployeeAdministration.Infrastructure.Options;
using EmployeeAdministration.Infrastructure.Options.Setups;
using EmployeeAdministration.Infrastructure.Services;
using EmployeeAdministration.Infrastructure.Services.Utils;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using User = EmployeeAdministration.Domain.Entities.User;

namespace EmployeeAdministration.Infrastructure;

public static class Startup
{
    public static void RegisterInfrastructure(this WebApplicationBuilder builder)
    {
        builder.RegisterDb();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.MapInboundClaims = false;
                });

        builder.Services.AddAuthorization();

        builder.Services.RegisterOptions();

        builder.Services.AddStackExchangeRedisCache(
            options => options.Configuration = builder.Configuration.GetConnectionString("Redis"));

        builder.Services.RegisterEmail(builder.Configuration);
        builder.Services.RegisterMessageBroker();

        builder.Services.RegisterInterfaces();
        builder.RegisterLogger();
    }

    private static void RegisterDb(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

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
    }

    private static void RegisterInterfaces(this IServiceCollection services)
    {
        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddTransient<IEventBus, EventBus>();

        services.AddScoped<IImagesStorageService, ImagesStorageService>();
        services.AddScoped<IWorkUnit, WorkUnit>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IServicesManager, ServicesManager>();
    }

    private static void RegisterOptions(this IServiceCollection services)
    {
        services.ConfigureOptions<CloudinaryOptionsSetup>();

        services.ConfigureOptions<JwtOptionsSetup>();
        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services.ConfigureOptions<EmailOptionsSetup>();
        services.ConfigureOptions<MessageBrokerOptionsSetup>();
    }

    private static void RegisterEmail(this IServiceCollection services, IConfiguration configuration)
    {
        EmailOptions emailOptions = new();
        configuration.GetSection(EmailOptions.SectionName).Bind(emailOptions);

        services.AddFluentEmail(emailOptions.Email)
                .AddSmtpSender(emailOptions.Host, emailOptions.Port, emailOptions.Email, emailOptions.Password);
    }

    private static void RegisterMessageBroker(this IServiceCollection services)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();
            config.AddConsumers(typeof(TaskCreatedEventConsumer));

            config.UsingRabbitMq((context, mqConfig) =>
            {
                var settings = context.GetRequiredService<IOptions<MessageBrokerOptions>>().Value;

                mqConfig.Host(new Uri(settings.HostName), host =>
                {
                    host.Username(settings.Username);
                    host.Password(settings.Password);
                });
            });
        });
    }

    private static void RegisterLogger(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration().ReadFrom
                                              .Configuration(builder.Configuration)
                                              .CreateLogger();

        builder.Logging.AddSerilog(Log.Logger);
        builder.Host.UseSerilog();
    }
}
