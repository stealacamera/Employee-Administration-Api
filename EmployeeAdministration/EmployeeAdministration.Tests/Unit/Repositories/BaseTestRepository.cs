using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Task = EmployeeAdministration.Domain.Entities.Task;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public abstract class BaseTestRepository
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<AppDbContext> _contextOptions;
    protected readonly AppDbContext _dbContext = null!;

    #region Dummy users
    protected static readonly User
        _deletedEmployee = new()
        {
            UserName = "deleted_user", Email = "deleteduser@email.com",
            FirstName = "Name", Surname = "Surname",
            DeletedAt = DateTime.UtcNow,
        },
        _employee = new()
        {
            UserName = "user_1", Email = "user_1@email.com",
            FirstName = "Name", Surname = "Surname",
        },
        _admin = new() 
        {
            UserName = "user_2", Email = "user_2@email.com",
            FirstName = "Name", Surname = "Surname"
        },
        _deletedAdmin = new()
        {
            UserName = "user_3", Email = "user_3@email.com",
            FirstName = "Name", Surname = "Surname"
        };

    public static readonly IEnumerable<object[]> _deletedUsers = 
        new List<object[]>() { new object[] { _deletedAdmin }, new object[] { _deletedEmployee }};
    
    public static readonly IEnumerable<object[]> _existingUsers = 
        new List<object[]>() { new object[] { _admin }, new object[] { _employee }};
    #endregion

    protected static readonly Project 
        _openProject = new()
        {
            CreatedAt = DateTime.UtcNow,
            Name = "Name",
            StatusId = ProjectStatuses.InProgress.Id
        },
        _finishedProject = new()
        {
            CreatedAt = DateTime.UtcNow,
            Name = "Name",
            StatusId = ProjectStatuses.Finished.Id
        },
        _pausedProject = new()
        {
            CreatedAt = DateTime.UtcNow,
            Name = "Name",
            StatusId = ProjectStatuses.Paused.Id
        },
        _emptyProject = new()
        {
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
            CreatedAt = DateTime.UtcNow
        };

    protected BaseTestRepository()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                            .UseSqlite(_connection)
                            .Options;

        using var dbContext = new AppDbContext(_contextOptions);
        SeedDummyData(dbContext);
        dbContext.SaveChanges();
    }

    private void SeedDummyData(AppDbContext dbContext)
    {
        dbContext.Users.Add(_employee);
        dbContext.Users.Add(_admin);
        dbContext.Users.Add(_deletedAdmin);
        dbContext.Users.Add(_deletedEmployee);

        dbContext.Projects.Add(_openProject);
        dbContext.Projects.Add(_finishedProject);
        dbContext.Projects.Add(_pausedProject);
        dbContext.Projects.Add(_emptyProject);

        dbContext.ProjectMembers.Add(_openProjectMember);
        dbContext.ProjectMembers.Add(_finishedProjectMember);
        dbContext.ProjectMembers.Add(_pausedProjectMember);

        dbContext.Tasks.Add(_finishedTask);
        dbContext.Tasks.Add(_openTask);

        dbContext.SaveChanges();
    }

    protected AppDbContext CreateContext() => new AppDbContext(_contextOptions);
    protected void Dispose() => _connection.Dispose();
}
