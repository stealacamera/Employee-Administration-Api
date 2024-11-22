using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = EmployeeAdministration.Domain.Entities.Task;

namespace EmployeeAdministration.Infrastructure;

public class AppDbContext : IdentityDbContext<User, Role, int>
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seeding roles
        modelBuilder.Entity<Role>(e =>
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

            e.HasData(roles);
        });
    }

    // Tables
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    
    public DbSet<Task> Tasks { get; set; }
}
