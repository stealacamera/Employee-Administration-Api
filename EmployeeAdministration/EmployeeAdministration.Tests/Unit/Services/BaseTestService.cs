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

    protected static User _deletedUser = new User { Id = 4, Email = "deleted@email.com", DeletedAt = DateTime.UtcNow },
                          _nonMemberEmployee = new User { Id = 1, Email = "user1@email.com" },
                          _memberEmployee = new User { Id = 2, Email = "user2@email.com" },
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
        new object[] { _deletedUser.Id },
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

        _mockWorkUnit.UsersRepository.IsUserInRoleAsync(_admin, Roles.Administrator).Returns(true);
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

        _mockWorkUnit.UsersRepository.GetByIdAsync(_deletedUser.Id).Returns(null as User);
        _mockWorkUnit.UsersRepository.GetByIdAsync(_deletedUser.Id, excludeDeletedUser: false).Returns(_deletedUser);
        _mockWorkUnit.UsersRepository.DoesUserExistAsync(_deletedUser.Id).Returns(true);
        _mockWorkUnit.UsersRepository.IsEmailInUseAsync(_deletedUser.Email, includeDeletedUsers: false).Returns(false);
        _mockWorkUnit.UsersRepository.IsEmailInUseAsync(_deletedUser.Email, includeDeletedUsers: true).Returns(true);
        _mockWorkUnit.UsersRepository.GetByEmailAsync(_deletedUser.Email).Returns(null as User);

        _mockWorkUnit.UsersRepository.GetByIdAsync(_nonMemberEmployee.Id).Returns(_nonMemberEmployee);
        _mockWorkUnit.UsersRepository.DoesUserExistAsync(_nonMemberEmployee.Id).Returns(true);
        _mockWorkUnit.UsersRepository.IsEmailInUseAsync(_nonMemberEmployee.Email, includeDeletedUsers: false).Returns(true);
        _mockWorkUnit.UsersRepository.IsEmailInUseAsync(_nonMemberEmployee.Email, includeDeletedUsers: true).Returns(true);
        _mockWorkUnit.UsersRepository.GetByEmailAsync(_nonMemberEmployee.Email).Returns(_nonMemberEmployee);

        _mockWorkUnit.UsersRepository.GetByIdAsync(_memberEmployee.Id).Returns(_memberEmployee);
        _mockWorkUnit.UsersRepository.DoesUserExistAsync(_memberEmployee.Id).Returns(true);
        _mockWorkUnit.UsersRepository.IsEmailInUseAsync(_memberEmployee.Email, includeDeletedUsers: false).Returns(true);
        _mockWorkUnit.UsersRepository.IsEmailInUseAsync(_memberEmployee.Email, includeDeletedUsers: true).Returns(true);
        _mockWorkUnit.UsersRepository.GetByEmailAsync(_memberEmployee.Email).Returns(_memberEmployee);

        _mockWorkUnit.UsersRepository.GetByIdAsync(_admin.Id).Returns(_admin);
        _mockWorkUnit.UsersRepository.DoesUserExistAsync(_admin.Id).Returns(true);
        _mockWorkUnit.UsersRepository.IsEmailInUseAsync(_admin.Email, includeDeletedUsers: false).Returns(true);
        _mockWorkUnit.UsersRepository.IsEmailInUseAsync(_admin.Email, includeDeletedUsers: true).Returns(true);
        _mockWorkUnit.UsersRepository.GetByEmailAsync(_admin.Email).Returns(_admin);
    }
}
