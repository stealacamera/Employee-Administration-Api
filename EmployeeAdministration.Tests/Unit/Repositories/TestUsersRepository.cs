using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure;
using EmployeeAdministration.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestUsersRepository
{
    private readonly AppDbContext _dbContext;
    private readonly UsersRepository _usersRepository;

    private User _employee, _deletedEmployee, _admin, _deletedAdmin;
    private User[] _allUsers, _undeletedUsers, _deletedUsers;

    public TestUsersRepository(UserManager<User> userManager, IDistributedCache distributedCache)
    {
        _usersRepository = new UsersRepository(userManager, distributedCache);

        SeedDummyData();
    }

    private void SeedDummyData()
    {
        var users = new[] { 
            (_employee, Roles.Employee, false),
            (_deletedEmployee, Roles.Employee, true),
            (_admin, Roles.Administrator, false),
            (_deletedAdmin, Roles.Administrator, true) 
        };

        for(int i = 0; i < users.Length; i++)
        {
            users[i].Item1 = _dbContext.Users
                             .Add(new User
                             {
                                 Id = i + 1,
                                 UserName = $"user-{i}",
                                 FirstName = "name",
                                 Surname = "surname",
                                 DeletedAt = users[i].Item3 ? DateTime.UtcNow : null
                             })
                             .Entity;

            _dbContext.UserRoles
                      .Add(new IdentityUserRole<int> 
                      { 
                          UserId = users[i].Item1.Id, 
                          RoleId = (int)users[i].Item2
                      });
        }

        _allUsers = [_employee, _deletedEmployee, _admin, _deletedAdmin];
        _undeletedUsers = [_employee, _admin];
        _deletedUsers = [_deletedEmployee, _deletedAdmin];
    }

    [Fact]
    public async Task DoesUserExist_UserExists_ReturnsTrue()
    {
        var result = await _usersRepository.DoesUserExistAsync(_employee.Id);
        Assert.True(result);
    }

    [Fact]
    public async Task DoesUserExistExcludeDeleted_UserIsSoftDeleted_ReturnsFalse()
    {
        var result = await _usersRepository.DoesUserExistAsync(_deletedEmployee.Id, includeDeletedUsers: false);
        Assert.False(result);
    }

    [Fact]
    public async Task DoesUserExistIncludeDeleted_UserIsSoftDeleted_ReturnsTrue()
    {
        var result = await _usersRepository.DoesUserExistAsync(_deletedEmployee.Id, includeDeletedUsers: true);
        Assert.True(result);
    }

    [Fact]
    public async Task DoesUserExist_UserDoesntExist_ReturnsFalse()
    {
        var result = await _usersRepository.DoesUserExistAsync(ushort.MaxValue);
        Assert.False(result);
    }

    [Theory]
    //[InlineData(null, _undeletedUsers)]
    //[InlineData(Roles.Employee, new[] { _employee })]
    //[InlineData(Roles.Administrator, new[] { _admin })]
    public async Task GetAllAsync_ExcludeDeleted_ReturnsUndeletedUsers(Roles? role, User[] expectedResult)
    {
        var result = await _usersRepository.GetAllAsync(includeDeletedUsers: false, filterByRole: role);

        Assert.Equal(expectedResult.Length, result.Count());
        Assert.Equal(expectedResult.Select(e => e.Id).ToArray(), result.Select(e => e.Id).ToArray());
    }

    [Theory]
    //[InlineData(null, _allUsers)]
    //[InlineData(Roles.Employee, )]
    //[InlineData(Roles.Administrator)]
    public async Task GetAllAsync_IncludeDeleted_ReturnsUndeletedUsers(Roles? role, User[] expectedResult)
    {
        var result = await _usersRepository.GetAllAsync(includeDeletedUsers: true, filterByRole: role);

        Assert.Equal(, result.Count());
        Assert.Equal(, result.Select(e => e.Id).ToArray());
    }

    // IsEmailInUse
        // user != null
        // user.del + ex/include
        // null

    public async Task IsEmailInUse_UserExists_ReturnsTrue()
    {
        var result = await _usersRepository.IsEmailInUseAsync(_employee.Email);
        Assert.True(result);
    }

    public async Task IsEmailInUse_UserDoesntExist_ReturnsFalse()
    {
        var result = await _usersRepository.IsEmailInUseAsync("nonexistinguser@email.com");
        Assert.False(result);
    }

    public async Task IsEmailInUseIncludeDeleted_UserIsSoftDeleted_ReturnsTrue()
    {
        var result = await _usersRepository.IsEmailInUseAsync(_deletedAdmin.Email, includeDeletedUsers: true);
        Assert.True(result);
    }

    public async Task IsEmailInUseExcludeDeleted_UserIsSoftDeleted_ReturnsFalse()
    {
        var result = await _usersRepository.IsEmailInUseAsync(_deletedAdmin.Email, includeDeletedUsers: false);
        Assert.False(result);
    }
}
