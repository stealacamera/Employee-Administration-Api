using EmployeeAdministration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeAdministration.Infrastructure.Configurations;

internal class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .IsRequired()   
               .HasMaxLength(150);

        builder.HasIndex(e => e.Name)
               .IsUnique();

        builder.Property(e => e.CreatedAt)
               .IsRequired();
    }
}
