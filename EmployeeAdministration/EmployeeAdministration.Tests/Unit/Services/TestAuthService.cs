using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure.Services;
using NSubstitute;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestAuthService : BaseTestService
{
    private static readonly string[] _roles = [nameof(Roles.Administrator), nameof(Roles.Employee)];
    private readonly AuthService _service;

    public static readonly IEnumerable<object[]>
        _userNotInRoleArguments = new List<object[]>
    {
        new object[] { _memberEmployee.Id, new[] { nameof(Roles.Administrator) } },
        new object[] { _admin.Id, new[] { nameof(Roles.Employee) } },
    },
        _userInRoleArguments = new List<object[]>
    {
        new object[] { _memberEmployee.Id, new[] { nameof(Roles.Employee) } },
        new object[] { _admin.Id, new[] { nameof(Roles.Administrator) } },
    },
        _deletedUserArguments = new List<object[]>
    {
        new object[] { _deletedEmployee.Id },
        new object[] { _deletedAdmin.Id },
    };

    public TestAuthService() : base()
        => _service = new(_mockWorkUnit, Substitute.For<IJwtProvider>());

    [Fact]
    public async Task IsUserAuthorized_NonexistingUser_ReturnsFalse()
        => Assert.False(
            await _service.IsUserAuthorizedAsync(_nonExistingEntityId, _roles));

    [Theory]
    [MemberData(nameof(_deletedUserArguments))]
    public async Task IsUserAuthorized_DeletedUser_ReturnsFalse(int userId)
        => Assert.False(
            await _service.IsUserAuthorizedAsync(userId, _roles));

    [Theory]
    [MemberData(nameof(_userNotInRoleArguments))]
    public async Task IsUserAuthorized_UserNotInRole_ReturnsFalse(int userId, string[] allowedRoles)
        => Assert.False(
            await _service.IsUserAuthorizedAsync(userId, allowedRoles));

    [Theory]
    [MemberData(nameof(_userInRoleArguments))]
    public async Task IsUserAuthorized_UserInRole_ReturnsTrue(int userId, string[] allowedRoles)
        => Assert.True(
            await _service.IsUserAuthorizedAsync(userId, allowedRoles));
}
