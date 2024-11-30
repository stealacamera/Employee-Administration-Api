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
        _deletedEmployee = new User { Id = 4, Email = "deleted@email.com", DeletedAt = DateTime.UtcNow },
        _nonMemberEmployee = new User { Id = 1, Email = "user1@email.com" },
        _memberEmployee = new User { Id = 2, Email = "user2@email.com" },
        _deletedAdmin = new() { Id = 5, Email = "deletedadmin@email.com", DeletedAt = DateTime.UtcNow },
        _admin = new User { Id = 3, Email = "admin@email.com" };

    protected static Project _projectWithOpenTasks = new Project { Id = 1, StatusId = ProjectStatuses.InProgress.Id },
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
        AppointeeEmployeeId = _memberEmployee.Id,
        AppointerUserId = _admin.Id,
        ProjectId = _projectWithOpenTasks.Id
    };
    #endregion

    public static readonly IEnumerable<object[]> _invalidUsersArguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId },
        new object[] { _deletedEmployee.Id },
        new object[] { _nonMemberEmployee.Id },
    };

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
        _mockWorkUnit.ProjectMembersRepository
                     .GetByIdsAsync(_memberEmployee.Id, _projectWithOpenTasks.Id)
                     .Returns(_openTasksMembership);

        _mockWorkUnit.ProjectMembersRepository
                     .IsUserMemberAsync(_memberEmployee.Id, _projectWithOpenTasks.Id)
                     .Returns(true);

        _mockWorkUnit.ProjectMembersRepository
                     .GetByIdsAsync(_memberEmployee.Id, _projectWithCompletedTasks.Id)
                     .Returns(_completedTasksMembership);

        _mockWorkUnit.ProjectMembersRepository
                     .IsUserMemberAsync(_memberEmployee.Id, _projectWithCompletedTasks.Id)
                     .Returns(true);
    }

    private void SeedDummyRoles()
    {
        _mockWorkUnit.UsersRepository.GetUserRoleAsync(_admin).Returns(Roles.Administrator);
        _mockWorkUnit.UsersRepository.GetUserRoleAsync(_nonMemberEmployee).Returns(Roles.Employee);
        _mockWorkUnit.UsersRepository.GetUserRoleAsync(_memberEmployee).Returns(Roles.Employee);
        _mockWorkUnit.UsersRepository.GetUserRoleAsync(_deletedEmployee).Returns(Roles.Employee);
        _mockWorkUnit.UsersRepository.GetUserRoleAsync(_deletedAdmin).Returns(Roles.Administrator);

        _mockWorkUnit.UsersRepository.IsUserInRoleAsync(_admin, Roles.Administrator).Returns(true);
        _mockWorkUnit.UsersRepository.IsUserInRoleAsync(_deletedAdmin, Roles.Administrator).Returns(true);
        _mockWorkUnit.UsersRepository.IsUserInRoleAsync(_deletedEmployee, Roles.Employee).Returns(true);
        _mockWorkUnit.UsersRepository.IsUserInRoleAsync(_nonMemberEmployee, Roles.Employee).Returns(true);
        _mockWorkUnit.UsersRepository.IsUserInRoleAsync(_memberEmployee, Roles.Employee).Returns(true);
    }

    private void SeedDummyProjects()
    {
        _mockWorkUnit.ProjectsRepository.GetByIdAsync(_projectWithOpenTasks.Id).Returns(_projectWithOpenTasks);
        _mockWorkUnit.ProjectsRepository.DoesInstanceExistAsync(_projectWithOpenTasks.Id).Returns(true);

        _mockWorkUnit.ProjectsRepository.GetByIdAsync(_projectWithCompletedTasks.Id).Returns(_projectWithCompletedTasks);
        _mockWorkUnit.ProjectsRepository.DoesInstanceExistAsync(_projectWithCompletedTasks.Id).Returns(true);

        _mockWorkUnit.ProjectsRepository.GetByIdAsync(_nonExistingEntityId).Returns(null as Project);
        _mockWorkUnit.ProjectsRepository.DoesInstanceExistAsync(_nonExistingEntityId).Returns(false);
    }

    private void SeedDummyTasks()
    {
        _mockWorkUnit.TasksRepository.GetByIdAsync(_nonExistingEntityId).Returns(null as Task);
        _mockWorkUnit.TasksRepository.GetByIdAsync(_adminAssignedTask.Id).Returns(_adminAssignedTask);

        _mockWorkUnit.TasksRepository
                     .DoesUserHaveOpenTasksAsync(_memberEmployee.Id)
                     .Returns(true);

        _mockWorkUnit.TasksRepository
                     .DoesUserHaveOpenTasksAsync(_memberEmployee.Id, _projectWithCompletedTasks.Id)
                     .Returns(false);

        _mockWorkUnit.TasksRepository
                     .DoesUserHaveOpenTasksAsync(_memberEmployee.Id, _projectWithOpenTasks.Id)
                     .Returns(true);

        _mockWorkUnit.TasksRepository
                     .DoesProjectHaveOpenTasksAsync(_projectWithOpenTasks.Id)
                     .Returns(true);

        _mockWorkUnit.TasksRepository
                     .DoesProjectHaveOpenTasksAsync(_projectWithCompletedTasks.Id)
                     .Returns(false);
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
            _mockWorkUnit.UsersRepository.DoesUserExistAsync(user.Id).Returns(true);
            _mockWorkUnit.UsersRepository.IsEmailInUseAsync(user.Email, includeDeletedUsers: false).Returns(true);
            _mockWorkUnit.UsersRepository.IsEmailInUseAsync(user.Email, includeDeletedUsers: true).Returns(true);
            _mockWorkUnit.UsersRepository.GetByEmailAsync(user.Email).Returns(user);
        }
    }
}
