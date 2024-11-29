using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestUsersRepository : BaseTestRepository
{
    private readonly UsersRepository _repository;

    public TestUsersRepository(IServiceProvider serviceProvider) : base()
    {
        CreateContext();

        _repository = new UsersRepository(
            serviceProvider.GetRequiredService<UserManager<User>>(), 
            Substitute.For<IDistributedCache>());
    }
    
    [Theory]
    [MemberData(nameof(_existingUsers))]
    public async Task DoesUserExist_UserExists_ReturnsTrue(User user)
    {
        var result = await _repository.DoesUserExistAsync(user.Id);
        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(_deletedUsers))]
    public async Task DoesUserExistExcludeDeleted_UserIsSoftDeleted_ReturnsFalse(User user)
    {
        var result = await _repository.DoesUserExistAsync(user.Id, includeDeletedUsers: false);
        Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(_deletedUsers))]
    public async Task DoesUserExistIncludeDeleted_UserIsSoftDeleted_ReturnsTrue(User user)
    {
        var result = await _repository.DoesUserExistAsync(user.Id, includeDeletedUsers: true);
        Assert.True(result);
    }

    [Fact]
    public async Task DoesUserExist_UserDoesntExist_ReturnsFalse()
    {
        var result = await _repository.DoesUserExistAsync(ushort.MaxValue);
        Assert.False(result);
    }

    public static readonly IEnumerable<object[]> _getAllUsersExcludeDeleted = new List<object[]> {
        new object[] { null, Array.Empty<User>() },
        new object[] { Roles.Employee, new[] { _employee } },
        new object[] { Roles.Administrator, new[] { _admin } },
    };

    [Theory]
    [MemberData(nameof(_getAllUsersExcludeDeleted))]
    public async Task GetAllAsync_ExcludeDeleted_ReturnsUndeletedUsers(Roles? role, User[] expectedResult)
    {
        var result = await _repository.GetAllAsync(includeDeletedUsers: false, filterByRole: role);

        Assert.Equal(expectedResult.Length, result.Count());
        Assert.Equal(expectedResult.Select(e => e.Id).ToArray(), result.Select(e => e.Id).ToArray());
    }

    public static readonly IEnumerable<object[]> _getAllUsersIncludeDeleted = new List<object[]> {
        new object[] { null, new[] { _employee, _deletedEmployee, _admin, _deletedAdmin } },
        new object[] { Roles.Employee, new[] { _employee, _deletedEmployee } },
        new object[] { Roles.Administrator, new[] { _admin, _deletedAdmin } },
    };

    [Theory]
    [MemberData(nameof(_getAllUsersIncludeDeleted))]
    public async Task GetAllAsync_IncludeDeleted_ReturnsUndeletedUsers(Roles? role, User[] expectedResult)
    {
        var result = await _repository.GetAllAsync(includeDeletedUsers: true, filterByRole: role);

        Assert.Equal(expectedResult.Length, result.Count());
        Assert.Equal(expectedResult.Select(e => e.Id).ToArray(), result.Select(e => e.Id).ToArray());
    }

    // IsEmailInUse
    // user != null
    // user.del + ex/include
    // null
    [Theory]
    [MemberData(nameof(_existingUsers))]
    public async Task IsEmailInUse_UserExists_ReturnsTrue(User user)
    {
        var result = await _repository.IsEmailInUseAsync(user.Email!);
        Assert.True(result);
    }

    [Fact]
    public async Task IsEmailInUse_UserDoesntExist_ReturnsFalse()
    {
        var result = await _repository.IsEmailInUseAsync("nonexistinguser@email.com");
        Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(_deletedUsers))]
    public async Task IsEmailInUseIncludeDeleted_UserIsSoftDeleted_ReturnsTrue(User user)
    {
        var result = await _repository.IsEmailInUseAsync(user.Email!, includeDeletedUsers: true);
        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(_deletedUsers))]
    public async Task IsEmailInUseExcludeDeleted_UserIsSoftDeleted_ReturnsFalse(User user)
    {
        var result = await _repository.IsEmailInUseAsync(user.Email!, includeDeletedUsers: false);
        Assert.False(result);
    }

    public static readonly IEnumerable<object[]> _getUserExcludeDeleted = new List<object[]> {
        new object[] { _deletedAdmin, null },
        new object[] { _deletedEmployee, null },
    };

    public static readonly IEnumerable<object[]> _getUserIncludeDeleted = new List<object[]> {
        new object[] { _deletedAdmin, _deletedAdmin },
        new object[] { _deletedEmployee, _deletedEmployee },
    };

    public static readonly IEnumerable<object[]> _getUser = new List<object[]>
    {
        new object[] { _admin, _admin },
        new object[] { _employee, _employee }
    };

    [Theory]
    [MemberData(nameof(_getUser))]
    [MemberData(nameof(_getUserExcludeDeleted))]
    public async Task GetByEmail_ExcludeDeleted_GetExistingUsers(User user, User? expectedUser)
    {
        var result = await _repository.GetByEmailAsync(user.Email!, excludeDeletedUser: true);
        Assert.Equal(expectedUser, result);
    }

    [Theory]
    [MemberData(nameof(_getUser))]
    [MemberData(nameof(_getUserIncludeDeleted))]
    public async Task GetByEmail_IncludeDeleted_GetAllUsers(User user, User? expectedUser)
    {
        var result = await _repository.GetByEmailAsync(user.Email!, excludeDeletedUser: false);
        Assert.Equal(expectedUser, result);
    }

    [Fact]
    public async Task GetById_NonexistingUser_ReturnsNull()
        => Assert.Null(await _repository.GetByIdAsync(ushort.MaxValue));

    [Theory]
    [MemberData(nameof(_getUser))]
    [MemberData(nameof(_getUserExcludeDeleted))]
    public async Task GetById_ExcludeDeletedUsers_ReturnsExistingUsers(User user, User? expectedUser)
    {
        var result = await _repository.GetByIdAsync(user.Id);
        Assert.Equal(expectedUser, result);
    }

    [Theory]
    [MemberData(nameof(_getUser))]
    [MemberData(nameof(_getUserIncludeDeleted))]
    public async Task GetById_IncludeDeletedUsers_ReturnsAllUsers(User user, User? expectedUser)
    {
        var result = await _repository.GetByIdAsync(user.Id);
        Assert.Equal(expectedUser, result);
    }
}