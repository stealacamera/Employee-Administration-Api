using EmployeeAdministration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = EmployeeAdministration.Domain.Entities.Task;

namespace EmployeeAdministration.Infrastructure.Configurations;

internal class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(e => e.Description)
               .HasMaxLength(350);

        builder.Property(e => e.IsCompleted)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
               .IsRequired();


        builder.Property(e => e.ProjectId)
               .IsRequired();

        builder.HasOne<Project>()
               .WithMany()
               .HasForeignKey(e => e.ProjectId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.AppointeeEmployeeId)
               .IsRequired();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(e => e.AppointeeEmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.AppointerEmployeeId)
               .IsRequired();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(e => e.AppointerEmployeeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
