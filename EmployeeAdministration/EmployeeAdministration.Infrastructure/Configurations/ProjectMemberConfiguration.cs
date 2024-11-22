using EmployeeAdministration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeAdministration.Infrastructure.Configurations;

internal class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(e => new { e.ProjectId, e.EmployeeId });

        builder.HasOne<Project>()
               .WithMany()
               .HasForeignKey(e => e.ProjectId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(e => e.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.CreatedAt)
               .IsRequired();
    }
}
