using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Task = EmployeeAdministration.Domain.Entities.Task;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public abstract class BaseTestRepository
{
    protected readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _contextOptions;
    private readonly AppDbContext _dbContext = null!;

    #region Dummy users
    protected static readonly User
        _deletedEmployee = new()
        {
            Id = 1,
            UserName = "deleted_user",
            Email = "user_4@email.com", NormalizedEmail = "USER_4@EMAIL.COM",
            FirstName = "Name",
            Surname = "Surname",
            DeletedAt = DateTime.UtcNow,
        },
        _employee = new()
        {
            Id = 2,
            UserName = "user_1",
            Email = "user_1@email.com", NormalizedEmail = "USER_1@EMAIL.COM",
            EmailConfirmed = true,
            FirstName = "Name",
            Surname = "Surname",
        },
        _admin = new()
        {
            Id = 3,
            UserName = "user_2",
            Email = "user_2@email.com",
            NormalizedEmail = "USER_2@EMAIL.COM",
            EmailConfirmed = true,
            FirstName = "Name",
            Surname = "Surname"
        },
        _deletedAdmin = new()
        {
            Id = 4,
            UserName = "user_3",
            Email = "user_3@email.com",
            NormalizedEmail = "USER_3@EMAIL.COM",
            EmailConfirmed = true,
            FirstName = "Name",
            Surname = "Surname",
            DeletedAt = DateTime.UtcNow,
        };

    private static readonly IdentityUserRole<int>
        _employeeRole = new() { RoleId = (int)Roles.Employee, UserId = _employee.Id },
        _deletedEmployeeRole = new() { RoleId = (int)Roles.Employee, UserId = _deletedEmployee.Id },
        _adminRole = new() { RoleId = (int)Roles.Administrator, UserId = _admin.Id },
        _deletedAdminRole = new() { RoleId = (int)Roles.Administrator, UserId = _deletedAdmin.Id };

    public static readonly IEnumerable<object[]> _deletedUsers =
        new List<object[]>() { new object[] { _deletedAdmin }, new object[] { _deletedEmployee } };

    public static readonly IEnumerable<object[]> _existingUsers =
        new List<object[]>() { new object[] { _admin }, new object[] { _employee } };
    #endregion

    protected static readonly Project
        _openProject = new()
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            Name = "Name",
            StatusId = ProjectStatuses.InProgress.Id
        },
        _finishedProject = new()
        {
            Id = 2,
            CreatedAt = DateTime.UtcNow,
            Name = "Name",
            StatusId = ProjectStatuses.Finished.Id
        },
        _pausedProject = new()
        {
            Id = 3,
            CreatedAt = DateTime.UtcNow,
            Name = "Name",
            StatusId = ProjectStatuses.Paused.Id
        },
        _emptyProject = new()
        {
            Id = 4,
            CreatedAt = DateTime.UtcNow,
            Name = "Name",
            StatusId = ProjectStatuses.Paused.Id
        };

    protected static readonly ProjectMember
        _openProjectMember = new()
        {
            CreatedAt = DateTime.UtcNow,
            EmployeeId = _employee.Id,
            ProjectId = _openProject.Id,
        },
        _finishedProjectMember = new()
        {
            CreatedAt = DateTime.UtcNow,
            EmployeeId = _employee.Id,
            ProjectId = _finishedProject.Id
        },
        _pausedProjectMember = new()
        {
            CreatedAt = DateTime.UtcNow,
            EmployeeId = _employee.Id,
            ProjectId = _pausedProject.Id
        };

    protected static readonly Task
        _openTask = new()
        {
            ProjectId = _openProject.Id,
            AppointeeEmployeeId = _employee.Id,
            AppointerUserId = _admin.Id,
            Name = "Name",
            CreatedAt = DateTime.UtcNow
        },
        _finishedTask = new()
        {
            ProjectId = _finishedProject.Id,
            AppointeeEmployeeId = _employee.Id,
            AppointerUserId = _admin.Id,
            Name = "Name",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow
        };

    protected BaseTestRepository()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                            .UseSqlite(_connection)
                            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                            .Options;

        using (var dbContext = new AppDbContext(_contextOptions))
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            SeedDummyData(dbContext);
            dbContext.SaveChanges();
        }
    }

    private void SeedDummyData(AppDbContext dbContext)
    {
        dbContext.Users.AddRange(_employee, _admin, _deletedAdmin, _deletedEmployee);
        dbContext.UserRoles.AddRange(_adminRole, _deletedAdminRole, _deletedEmployeeRole, _employeeRole);
        dbContext.SaveChanges();

        dbContext.Projects.AddRange(_openProject, _finishedProject, _pausedProject, _emptyProject);
        dbContext.SaveChanges();

        dbContext.ProjectMembers.AddRange(_openProjectMember, _finishedProjectMember, _pausedProjectMember);
        dbContext.SaveChanges();

        dbContext.Tasks.AddRange(_finishedTask, _openTask);
        dbContext.SaveChanges();
    }

    protected AppDbContext CreateContext() => new AppDbContext(_contextOptions);
    protected void Dispose() => _connection.Dispose();
}
