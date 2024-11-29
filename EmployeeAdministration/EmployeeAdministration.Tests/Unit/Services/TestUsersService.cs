using System.ComponentModel.DataAnnotations;
using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services.Utils;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Services;
using EmployeeAdministration.Domain.Enums;
using NSubstitute;
using Task = System.Threading.Tasks.Task;
using ValidationException = EmployeeAdministration.Application.Common.Exceptions.ValidationException;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestUsersService : BaseTestService
{
    private readonly UsersService _service;
    private readonly UpdateUserRequest _dummyUpdateUserRequest = new("Test name");
    private readonly UpdatePasswordRequest _dummyUpdatePasswordRequest = new(_usersPassword, "new_password");

    public TestUsersService() : base()
        => _service = new(
                _mockWorkUnit,
                Substitute.For<IJwtProvider>(),
                Substitute.For<IImagesService>());

    [Fact]
    public async Task Create_ExistingEmail_ThrowsError()
    {
        var request = new CreateUserRequest(
            Roles.Employee, _nonMemberEmployee.Email!,
            "Name", "Surname", "password");

        await Assert.ThrowsAsync<ValidationException>(
            async () => await _service.CreateUserAsync(request));
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsUser()
    {
        var request = new CreateUserRequest(
            Roles.Employee, "newemail@email.com",
            "Name", "Surname", "password");

        var result = await Record.ExceptionAsync(
            async () => await _service.CreateUserAsync(request));

        Assert.Null(result);
    }

    public static readonly IEnumerable<object[]> _deleteUserArguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId, typeof(EntityNotFoundException) },
        new object[] { _deletedUser.Id, typeof(EntityNotFoundException) },
        new object[] { _memberEmployee.Id, typeof(UncompletedTasksAssignedToEntityException) },
    };

    [Theory]
    [MemberData(nameof(_deleteUserArguments))]
    public async Task Delete_InvalidUser_ThrowsError(int userId, Type exceptionExpected)
        => await Assert.ThrowsAsync(exceptionExpected, async () => await _service.DeleteUserAsync(userId));

    [Fact]
    public async Task Delete_ValidRequest_DeletesUserAndMemberships()
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.DeleteUserAsync(_nonMemberEmployee.Id));

        Assert.Null(result);
    }

    public static readonly IEnumerable<object[]> _updateUser_InvalidRequester_Arguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId },
        new object[] { _deletedUser.Id },
        new object[] { _nonMemberEmployee.Id },
    };

    [Theory]
    [MemberData(nameof(_updateUser_InvalidRequester_Arguments))]
    public async Task Update_InvalidRequester_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _service.UpdateUserAsync(requesterId, _memberEmployee.Id, _dummyUpdateUserRequest));

    public static readonly IEnumerable<object[]> _updateUser_InvalidUser_Arguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId },
        new object[] { _deletedUser.Id },
    };

    [Theory]
    [MemberData(nameof(_updateUser_InvalidUser_Arguments))]
    public async Task Update_InvalidUser_ThrowsError(int userId)
        => await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _service.UpdateUserAsync(_admin.Id, userId, _dummyUpdateUserRequest));

    public static readonly IEnumerable<object[]> _updateUser_ValidRequest_Arguments = new List<object[]>
    {
        new object[] { _admin.Id },
        new object[] { _nonMemberEmployee.Id },
    };

    [Theory]
    [MemberData(nameof(_updateUser_ValidRequest_Arguments))]
    public async Task Update_ValidRequest_ReturnsUpdatedUser(int requesterId)
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.UpdateUserAsync(requesterId, _nonMemberEmployee.Id, _dummyUpdateUserRequest));

        Assert.Null(result);
    }

    public static readonly IEnumerable<object[]> _verifyCredentials_InvalidRequest_Arguments = new List<object[]>
    {
        new object[] { "nonexistinguser@email.com", _usersPassword },
        new object[] { _memberEmployee.Email, "wrong_password" },
    };

    [Theory]
    [MemberData(nameof(_verifyCredentials_InvalidRequest_Arguments))]
    public async Task VerifyCredentials_InvalidRequest_ReturnsNull(string email, string password)
        => Assert.Null(await _service.VerifyCredentialsAsync(new(email, password)));

    [Fact]
    public async Task VerifyCredentials_ValidRequest_ReturnsUser()
    {
        _mockWorkUnit.UsersRepository
                     .VerifyCredentialsAsync(_memberEmployee, _usersPassword)
                     .Returns(true);

        Assert.NotNull(await _service.VerifyCredentialsAsync(new(_memberEmployee.Email, _usersPassword)));
    }

    public static readonly IEnumerable<object[]> _updatePassword_InvalidRequester_Arguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId },
        new object[] { _deletedUser.Id },
    };

    [Theory]
    [MemberData(nameof(_updatePassword_InvalidRequester_Arguments))]
    public async Task UpdatePassword_InvalidRequester_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.UpdatePasswordAsync(requesterId, _dummyUpdatePasswordRequest));

    [Fact]
    public async Task UpdatePassword_InvalidCurrentPassword_ThrowsError()
    {
        _mockWorkUnit.UsersRepository
                     .VerifyCredentialsAsync(_memberEmployee, _usersPassword)
                     .Returns(true);

        await Assert.ThrowsAsync<InvalidPasswordException>(
            async () => await _service.UpdatePasswordAsync(_memberEmployee.Id, new("wrong_password", "new_password")));
    }

    [Fact]
    public async Task UpdatePassword_ValidRequest_DoesNotThrowError()
    {
        _mockWorkUnit.UsersRepository
                     .VerifyCredentialsAsync(_memberEmployee, _usersPassword)
                     .Returns(true);

        var result = await Record.ExceptionAsync(
            async () => await _service.UpdatePasswordAsync(_memberEmployee.Id, _dummyUpdatePasswordRequest));

        Assert.Null(result);
    }
}
