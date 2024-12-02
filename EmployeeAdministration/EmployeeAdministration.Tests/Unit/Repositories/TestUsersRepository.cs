using EmployeeAdministration.Domain.Entities;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure;
using EmployeeAdministration.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestUsersRepository : BaseTestRepository
{
    private UserManager<User> _userManager;
    private IDistributedCache _distributedCache = Substitute.For<IDistributedCache>();
    private UsersRepository _repository = null!;
    private readonly IServiceCollection _services = new ServiceCollection();

    public static readonly IEnumerable<object[]>
        _deletedUsers = new List<object[]>() 
    {
        new object[] { _deletedAdmin }, 
        new object[] { _deletedEmployee } 
    },
        _existingUsers = new List<object[]>() 
    { 
        new object[] { _admin }, 
        new object[] { _employee } 
    },
    _getAllUsersExcludeDeletedArguments = new List<object[]>
    {
        new object[] { null, new[] { _employee, _admin } },
        new object[] { Roles.Employee, new[] { _employee } },
        new object[] { Roles.Administrator, new[] { _admin } },
    },
        _getAllUsersIncludeDeletedArguments = new List<object[]>
    {
        new object[] { null, new[] { _employee, _deletedEmployee, _admin, _deletedAdmin } },
        new object[] { Roles.Employee, new[] { _employee, _deletedEmployee } },
        new object[] { Roles.Administrator, new[] { _admin, _deletedAdmin } },
    },
        _getUserExcludeDeletedArguments = new List<object[]>
    {
        new object[] { _deletedAdmin, null },
        new object[] { _deletedEmployee, null },
    },
        _getUserIncludeDeletedArguments = new List<object[]>
    {
        new object[] { _deletedAdmin, _deletedAdmin },
        new object[] { _deletedEmployee, _deletedEmployee },
    },
        _getExistingUserArguments = new List<object[]>
    {
        new object[] { _admin, _admin },
        new object[] { _employee, _employee }
    };

    public TestUsersRepository() : base()
    {
        _services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

        _services.AddIdentityCore<User>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;

            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";

            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<AppDbContext>();

        _userManager = _services.BuildServiceProvider()
                                   .GetRequiredService<UserManager<User>>();
    }

    [Theory]
    [MemberData(nameof(_existingUsers))]
    public async Task DoesUserExist_UserExists_ReturnsTrue(User user)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.DoesUserExistAsync(user.Id);
        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(_deletedUsers))]
    public async Task DoesUserExistExcludeDeleted_UserIsSoftDeleted_ReturnsFalse(User user)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.DoesUserExistAsync(user.Id, includeDeletedUsers: false);
        Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(_deletedUsers))]
    public async Task DoesUserExistIncludeDeleted_UserIsSoftDeleted_ReturnsTrue(User user)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.DoesUserExistAsync(user.Id, includeDeletedUsers: true);
        Assert.True(result);
    }

    [Fact]
    public async Task DoesUserExist_UserDoesntExist_ReturnsFalse()
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.DoesUserExistAsync(ushort.MaxValue);
        Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(_getAllUsersExcludeDeletedArguments))]
    public async Task GetAllAsync_ExcludeDeleted_ReturnsUndeletedUsers(Roles? role, User[] expectedResult)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.GetAllAsync(includeDeletedUsers: false, filterByRole: role);
        var resultIds = result.Select(e => e.Id).ToArray();

        Assert.Equal(expectedResult.Length, result.Count());

        foreach (var item in expectedResult)
            Assert.Contains(item.Id, resultIds);
    }

    [Theory]
    [MemberData(nameof(_getAllUsersIncludeDeletedArguments))]
    public async Task GetAllAsync_IncludeDeleted_ReturnsUndeletedUsers(Roles? role, User[] expectedResult)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.GetAllAsync(includeDeletedUsers: true, filterByRole: role);
        var resultIds = result.Select(e => e.Id).ToArray();

        Assert.Equal(expectedResult.Length, result.Count());

        foreach (var item in expectedResult)
            Assert.Contains(item.Id, resultIds);
    }

    [Theory]
    [MemberData(nameof(_existingUsers))]
    public async Task IsEmailInUse_UserExists_ReturnsTrue(User user)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.IsEmailInUseAsync(user.Email!);
        Assert.True(result);
    }

    [Fact]
    public async Task IsEmailInUse_UserDoesntExist_ReturnsFalse()
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.IsEmailInUseAsync("nonexistinguser@email.com");
        Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(_deletedUsers))]
    public async Task IsEmailInUseIncludeDeleted_UserIsSoftDeleted_ReturnsTrue(User user)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.IsEmailInUseAsync(user.Email!, includeDeletedUsers: true);
        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(_deletedUsers))]
    public async Task IsEmailInUseExcludeDeleted_UserIsSoftDeleted_ReturnsFalse(User user)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.IsEmailInUseAsync(user.Email!, includeDeletedUsers: false);
        Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(_getExistingUserArguments))]
    [MemberData(nameof(_getUserExcludeDeletedArguments))]
    public async Task GetByEmail_ExcludeDeleted_GetExistingUsers(User user, User? expectedUser)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.GetByEmailAsync(user.Email!, excludeDeletedUser: true);
        Assert.Equal(expectedUser?.Id, result?.Id);
    }

    [Theory]
    [MemberData(nameof(_getExistingUserArguments))]
    [MemberData(nameof(_getUserIncludeDeletedArguments))]
    public async Task GetByEmail_IncludeDeleted_GetAllUsers(User user, User? expectedUser)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.GetByEmailAsync(user.Email!, excludeDeletedUser: false);
        Assert.Equal(expectedUser?.Id, result?.Id);
    }

    [Fact]
    public async Task GetById_NonexistingUser_ReturnsNull()
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        Assert.Null(await _repository.GetByIdAsync(ushort.MaxValue));
    }

    [Theory]
    [MemberData(nameof(_getExistingUserArguments))]
    [MemberData(nameof(_getUserExcludeDeletedArguments))]
    public async Task GetById_ExcludeDeletedUsers_ReturnsExistingUsers(User user, User? expectedUser)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.GetByIdAsync(user.Id);
        Assert.Equal(expectedUser?.Id, result?.Id);
    }

    [Theory]
    [MemberData(nameof(_getExistingUserArguments))]
    [MemberData(nameof(_getUserIncludeDeletedArguments))]
    public async Task GetById_IncludeDeletedUsers_ReturnsAllUsers(User user, User? expectedUser)
    {
        using var context = CreateContext();
        _repository = new UsersRepository(_userManager);

        var result = await _repository.GetByIdAsync(user.Id, excludeDeletedUser: false);
        Assert.Equal(expectedUser?.Id, result?.Id);
    }
}