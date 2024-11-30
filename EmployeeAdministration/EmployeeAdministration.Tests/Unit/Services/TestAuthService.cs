using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestAuthService : BaseTestService
{
    private static readonly string[] _roles = [nameof(Roles.Administrator), nameof(Roles.Employee)];
    private readonly AuthService _service;

    public TestAuthService() : base()
        => _service = new(_mockWorkUnit);

    [Fact]
    public async Task IsUserAuthorized_NonexistingUser_ReturnsFalse()
        => Assert.False(
            await _service.IsUserAuthorizedAsync(_nonExistingEntityId, _roles));

    public static readonly IEnumerable<object[]> _isUserAuthorized_DeletedUser = new List<object[]>
    {
        new object[] { _deletedEmployee.Id }, new object[] { _deletedAdmin.Id },
    };

    [Theory]
    [MemberData(nameof(_isUserAuthorized_DeletedUser))]
    public async Task IsUserAuthorized_DeletedUser_ReturnsFalse(int userId)
        => Assert.False(
            await _service.IsUserAuthorizedAsync(userId, _roles));

    public static readonly IEnumerable<object[]> _isUserAuthorized_UserNotInRole_Arguments = new List<object[]>
    {
        new object[] { _memberEmployee.Id, new[] { nameof(Roles.Administrator) } }, 
        new object[] { _admin.Id, new[] { nameof(Roles.Employee) } },
    };

    [Theory]
    [MemberData(nameof(_isUserAuthorized_UserNotInRole_Arguments))]
    public async Task IsUserAuthorized_UserNotInRole_ReturnsFalse(int userId, string[] allowedRoles)
        => Assert.False(
            await _service.IsUserAuthorizedAsync(userId, allowedRoles));

    public static readonly IEnumerable<object[]> _isUserAuthorized_UserInRole_Arguments = new List<object[]>
    {
        new object[] { _memberEmployee.Id, new[] { nameof(Roles.Employee) } },
        new object[] { _admin.Id, new[] { nameof(Roles.Administrator) } },
    };

    [Theory]
    [MemberData(nameof(_isUserAuthorized_UserInRole_Arguments))]
    public async Task IsUserAuthorized_UserInRole_ReturnsTrue(int userId, string[] allowedRoles)
        => Assert.True(
            await _service.IsUserAuthorizedAsync(userId, allowedRoles));

}
