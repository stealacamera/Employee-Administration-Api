using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EmployeeAdministration.Infrastructure.Options.Setups;

internal class MessageBrokerOptionsSetup : IConfigureOptions<MessageBrokerOptions>
{
    private readonly IConfiguration _configuration;

    public MessageBrokerOptionsSetup(IConfiguration configuration)
        => _configuration = configuration;

    public void Configure(MessageBrokerOptions options)
        => _configuration.GetSection(MessageBrokerOptions.SectionName).Bind(options);
}