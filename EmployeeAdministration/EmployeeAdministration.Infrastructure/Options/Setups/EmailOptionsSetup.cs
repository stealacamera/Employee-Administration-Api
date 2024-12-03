using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EmployeeAdministration.Infrastructure.Options.Setups;

internal class EmailOptionsSetup : IConfigureOptions<EmailOptions>
{
    private readonly IConfiguration _configuration;

    public EmailOptionsSetup(IConfiguration configuration)
        => _configuration = configuration;

    public void Configure(EmailOptions options)
        => _configuration.GetSection(EmailOptions.SectionName).Bind(options);
}
