using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EmployeeAdministration.Infrastructure.Options.Setups;

internal class CloudinaryOptionsSetup : IConfigureOptions<CloudinaryOptions>
{
    private readonly IConfiguration _configuration;

    public CloudinaryOptionsSetup(IConfiguration configuration)
        => _configuration = configuration;

    public void Configure(CloudinaryOptions options)
        => _configuration.GetSection(CloudinaryOptions.SectionName).Bind(options);
}
