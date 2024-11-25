using EmployeeAdministration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeAdministration.Infrastructure.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.ProfilePictureName)
               .HasMaxLength(400);

        builder.Property(e => e.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.Surname)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.Email)
               .IsRequired();

        builder.HasOne<Role>()
               .WithMany()
               .HasForeignKey(e => e.RoleId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
