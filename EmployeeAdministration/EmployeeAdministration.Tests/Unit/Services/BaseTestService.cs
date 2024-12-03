using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using NSubstitute;
using Task = EmployeeAdministration.Domain.Entities.Task;

namespace EmployeeAdministration.Tests.Unit.Services;

public abstract class BaseTestService
{
    protected readonly IWorkUnit _mockWorkUnit;

    #region Dummy data
    protected static int _nonExistingEntityId = 0;
    protected static string _usersPassword = "correct_password";

    protected static User
        _nonMemberEmployee = new User { Id = 1, Email = "user1@email.com" },
        _memberEmployee = new User { Id = 2, Email = "user2@email.com" },
        _admin = new User { Id = 3, Email = "admin@email.com" },
        _deletedEmployee = new User { Id = 4, Email = "deleted@email.com", DeletedAt = DateTime.UtcNow },
        _deletedAdmin = new() { Id = 5, Email = "deletedadmin@email.com", DeletedAt = DateTime.UtcNow };

    protected static Project
        _projectWithOpenTasks = new Project { Id = 1, StatusId = ProjectStatuses.InProgress.Id },
        _projectWithCompletedTasks = new Project { Id = 2, StatusId = ProjectStatuses.InProgress.Id };

    protected static ProjectMember
        _openTasksMembership = new()
        {
            ProjectId = _projectWithOpenTasks.Id,
            EmployeeId = _memberEmployee.Id
        },
        _completedTasksMembership = new()
        {
            ProjectId = _projectWithCompletedTasks.Id,
            EmployeeId = _memberEmployee.Id
        };

    protected static Task _adminAssignedTask = new Task
    {
        Id = 1,
        ProjectId = _projectWithOpenTasks.Id,
        AppointeeEmployeeId = _memberEmployee.Id,
        AppointerUserId = _admin.Id,
    };
    #endregion

    protected BaseTestService()
    {
        _mockWorkUnit = Substitute.For<IWorkUnit>();
        MockWorkUnitData();
    }

    private void MockWorkUnitData()
    {
        SeedDummyProjects();
        SeedDummyUsers();
        SeedDummyRoles();
        SeedDummyMemberships();
        SeedDummyTasks();
    }

    private void SeedDummyMemberships()
    {
        var data = new[]
        {
            (_memberEmployee, _projectWithOpenTasks, _openTasksMembership),
            (_memberEmployee, _projectWithCompletedTasks, _completedTasksMembership)
        };

        foreach (var (user, project, membership) in data)
        {
            _mockWorkUnit.ProjectMembersRepository
                         .GetByIdsAsync(user.Id, project.Id)
                         .Returns(membership);

            _mockWorkUnit.ProjectMembersRepository
                         .IsUserMemberAsync(user.Id, project.Id)
                         .Returns(true);
        }
    }

    private void SeedDummyRoles()
    {
        var userRoles = new[]
        {
            (_admin, Roles.Administrator),
            (_deletedAdmin, Roles.Administrator),
            (_deletedEmployee, Roles.Employee),
            (_memberEmployee, Roles.Employee),
            (_nonMemberEmployee, Roles.Employee),
        };

        foreach (var (user, role) in userRoles)
        {
            _mockWorkUnit.UserRolesRepository.GetUserRoleAsync(user.Id).Returns(role);
            _mockWorkUnit.UserRolesRepository.IsUserInRoleAsync(user.Id, role).Returns(true);
        }
    }

    private void SeedDummyProjects()
    {
        _mockWorkUnit.ProjectsRepository.GetByIdAsync(_nonExistingEntityId).Returns(null as Project);
        _mockWorkUnit.ProjectsRepository.DoesInstanceExistAsync(_nonExistingEntityId).Returns(false);
        
        foreach (var project in new[] { _projectWithOpenTasks, _projectWithCompletedTasks })
        {
            _mockWorkUnit.ProjectsRepository.GetByIdAsync(project.Id).Returns(project);
            _mockWorkUnit.ProjectsRepository.DoesInstanceExistAsync(project.Id).Returns(true);
        }
    }

    private void SeedDummyTasks()
    {
        _mockWorkUnit.TasksRepository.GetByIdAsync(_nonExistingEntityId).Returns(null as Task);
        _mockWorkUnit.TasksRepository.GetByIdAsync(_adminAssignedTask.Id).Returns(_adminAssignedTask);

        _mockWorkUnit.TasksRepository
                     .DoesUserHaveOpenTasksAsync(_memberEmployee.Id)
                     .Returns(true);

        var data = new[] 
        { 
            (_memberEmployee, _projectWithOpenTasks, true), 
            (_memberEmployee, _projectWithCompletedTasks, false) 
        };

        foreach (var (user, project, hasOpenTasks) in data)
        {
            _mockWorkUnit.TasksRepository
                         .DoesUserHaveOpenTasksAsync(user.Id, project.Id)
                         .Returns(hasOpenTasks);

            _mockWorkUnit.TasksRepository
                         .DoesProjectHaveOpenTasksAsync(project.Id)
                         .Returns(hasOpenTasks);
        }
    }

    private void SeedDummyUsers()
    {
        _mockWorkUnit.UsersRepository.GetByIdAsync(_nonExistingEntityId).Returns(null as User);
        _mockWorkUnit.UsersRepository.DoesUserExistAsync(_nonExistingEntityId).Returns(false);

        foreach (var deletedUser in new[] { _deletedAdmin, _deletedEmployee })
        {
            _mockWorkUnit.UsersRepository.GetByIdAsync(deletedUser.Id).Returns(null as User);
            _mockWorkUnit.UsersRepository.GetByIdAsync(deletedUser.Id, excludeDeletedUser: false).Returns(deletedUser);
            _mockWorkUnit.UsersRepository.DoesUserExistAsync(deletedUser.Id).Returns(true);
            _mockWorkUnit.UsersRepository.IsEmailInUseAsync(deletedUser.Email, includeDeletedUsers: false).Returns(false);
            _mockWorkUnit.UsersRepository.IsEmailInUseAsync(deletedUser.Email, includeDeletedUsers: true).Returns(true);
            _mockWorkUnit.UsersRepository.GetByEmailAsync(deletedUser.Email).Returns(null as User);
        }

        foreach (var user in new[] { _memberEmployee, _nonMemberEmployee, _admin })
        {
            _mockWorkUnit.UsersRepository.GetByIdAsync(user.Id).Returns(user);
            _mockWorkUnit.UsersRepository.GetByIdAsync(user.Id, excludeDeletedUser: false).Returns(user);
            _mockWorkUnit.UsersRepository.DoesUserExistAsync(user.Id).Returns(true);
            _mockWorkUnit.UsersRepository.IsEmailInUseAsync(user.Email, includeDeletedUsers: false).Returns(true);
            _mockWorkUnit.UsersRepository.IsEmailInUseAsync(user.Email, includeDeletedUsers: true).Returns(true);
            _mockWorkUnit.UsersRepository.GetByEmailAsync(user.Email).Returns(user);
        }
    }
}
