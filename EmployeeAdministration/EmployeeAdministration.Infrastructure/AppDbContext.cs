using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = EmployeeAdministration.Domain.Entities.Task;

namespace EmployeeAdministration.Infrastructure;

public class AppDbContext : IdentityDbContext<User, Role, int>
{
    #region Tables
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectStatus> ProjectStatuses { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<Task> Tasks { get; set; }
    #endregion

    public AppDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        SeedRoles(modelBuilder);
        SeedProjectStatus(modelBuilder);
    }

    private void SeedProjectStatus(ModelBuilder modelBuilder)
    {
        var statuses = Enum.GetValues<ProjectStatuses>()
                            .Select((e, index) => new ProjectStatus
                            {
                                Id = (sbyte)(index + 1),
                                Name = e.ToString(),
                                Value = (sbyte)e
                            })
                            .ToArray();

        modelBuilder.Entity<ProjectStatus>().HasData(statuses);
    }

    private void SeedRoles(ModelBuilder modelBuilder)
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

        modelBuilder.Entity<Role>().HasData(roles);
    }
}
