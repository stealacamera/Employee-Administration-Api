using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeAdministration.Infrastructure.Configurations;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        var roles = Enum.GetValues<Roles>()
                        .Select(e => new Role
                        {
                            Id = (int)e,
                            ConcurrencyStamp = e.ToString(),
                            Name = e.ToString(),
                            NormalizedName = e.ToString().ToUpper()
                        })
                        .ToArray();

        builder.HasData(roles);
    }
}
