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
                            Id = (sbyte)e,
                            ConcurrencyStamp = e.ToString(),
                            Name = Enum.GetName(e)!,
                            NormalizedName = Enum.GetName(e)!.ToUpper()
                        })
                        .ToArray();

        builder.HasData(roles);
    }
}
