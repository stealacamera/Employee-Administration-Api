﻿using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services.Utils;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Infrastructure.Services;
using EmployeeAdministration.Domain.Enums;
using NSubstitute;
using Task = System.Threading.Tasks.Task;
using ValidationException = EmployeeAdministration.Application.Common.Exceptions.General.ValidationException;
using EmployeeAdministration.Application.Common.Exceptions.General;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestUsersService : BaseTestService
{
    private readonly UsersService _service;
    private readonly UpdateUserRequest _dummyUpdateUserRequest = new("Test name");
    private readonly UpdatePasswordRequest _dummyUpdatePasswordRequest = new(_usersPassword, "new_password");

    public static readonly IEnumerable<object[]>
        _create_ExistingEmail_Arguments = new List<object[]>
    {
        new object[] { _nonMemberEmployee.Email! },
        new object[] { _deletedEmployee.Email! }
    },
        _deleteUserArguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId, typeof(EntityNotFoundException) },
        new object[] { _deletedEmployee.Id, typeof(EntityNotFoundException) },
        new object[] { _memberEmployee.Id, typeof(UncompletedTasksAssignedToEntityException) },
    },
        _updateUser_InvalidRequester_Arguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId },
        new object[] { _deletedEmployee.Id },
        new object[] { _nonMemberEmployee.Id },
    },
        _invalidUpdateRequesterArguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId },
        new object[] { _deletedEmployee.Id },
    },
        _updateUser_ValidRequest_Arguments = new List<object[]>
    {
        new object[] { _admin.Id },
        new object[] { _nonMemberEmployee.Id },
    },
        _verifyCredentials_InvalidRequest_Arguments = new List<object[]>
    {
        new object[] { "nonexistinguser@email.com", _usersPassword },
        new object[] { _memberEmployee.Email, "wrong_password" },
    };

    public TestUsersService() : base()
        => _service = new(_mockWorkUnit, Substitute.For<IJwtProvider>(), Substitute.For<IImagesStorageService>());

    [Theory]
    [MemberData(nameof(_create_ExistingEmail_Arguments))]
    public async Task Create_ExistingEmail_ThrowsError(string email)
    {
        var request = new CreateUserRequest(
            Roles.Employee, email, "Name", "Surname", "password");

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

    [Theory]
    [MemberData(nameof(_deleteUserArguments))]
    public async Task Delete_InvalidUser_ThrowsError(int userId, Type exceptionExpected)
        => await Assert.ThrowsAsync(
                exceptionExpected, async () => await _service.DeleteUserAsync(userId));

    [Fact]
    public async Task Delete_ValidRequest_DeletesUserAndMemberships()
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.DeleteUserAsync(_nonMemberEmployee.Id));

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(_updateUser_InvalidRequester_Arguments))]
    public async Task Update_InvalidRequester_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _service.UpdateUserAsync(requesterId, _memberEmployee.Id, _dummyUpdateUserRequest));

    [Theory]
    [MemberData(nameof(_invalidUpdateRequesterArguments))]
    public async Task Update_InvalidUser_ThrowsError(int userId)
        => await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                await _service.UpdateUserAsync(_admin.Id, userId, _dummyUpdateUserRequest));

    [Theory]
    [MemberData(nameof(_updateUser_ValidRequest_Arguments))]
    public async Task Update_ValidRequest_ReturnsUpdatedUser(int requesterId)
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.UpdateUserAsync(requesterId, _nonMemberEmployee.Id, _dummyUpdateUserRequest));

        Assert.Null(result);
    }

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

    [Theory]
    [MemberData(nameof(_invalidUpdateRequesterArguments))]
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
                     .IsPasswordCorrectAsync(_memberEmployee, _usersPassword)
                     .Returns(true);

        var result = await Record.ExceptionAsync(
            async () => await _service.UpdatePasswordAsync(_memberEmployee.Id, _dummyUpdatePasswordRequest));

        Assert.Null(result);
    }
}
