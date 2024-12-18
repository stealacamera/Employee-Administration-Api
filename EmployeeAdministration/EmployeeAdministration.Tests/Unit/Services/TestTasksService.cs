﻿using Azure.Core;
using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Common.Exceptions.General;
using EmployeeAdministration.Infrastructure.Services;
using NSubstitute;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestTasksService : BaseTestService
{
    private readonly TasksService _service;
    private readonly UpdateTaskRequest _dummyUpdateRequest = new("Test name");
    private readonly CreateTaskRequest _dummyCreateRequest = new("Test name", _memberEmployee.Id);

    public static readonly IEnumerable<object[]> 
        _invalidUsersArguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId },
        new object[] { _deletedEmployee.Id },
        new object[] { _nonMemberEmployee.Id },
    }, 
        _create_InvalidAppointee_Arguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId, typeof(EntityNotFoundException) },
        new object[] { _deletedEmployee.Id, typeof(EntityNotFoundException) },
        new object[] { _admin.Id, typeof(NonEmployeeUserException) },
        new object[] { _nonMemberEmployee.Id, typeof(NotAProjectMemberException) },
    },
        _validUserArguments = new List<object[]>
    {
        new object[] { _admin.Id },
        new object[] { _memberEmployee.Id },
    },
        _validTaskRequesterArguments = new List<object[]>
    {
        new object[] { _admin.Id },
        new object[] { _adminAssignedTask.AppointeeEmployeeId },
    };

    public TestTasksService() : base()
        => _service = new TasksService(_mockWorkUnit, Substitute.For<IEventBus>());

    [Theory]
    [MemberData(nameof(_invalidUsersArguments))]
    public async Task Create_AppointerIsInvalid_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.CreateAsync(requesterId, _projectWithOpenTasks.Id, _dummyCreateRequest));

    [Theory]
    [MemberData(nameof(_create_InvalidAppointee_Arguments))]
    public async Task Create_AppointeeIsInvalid_ThrowsError(int appointeeId, Type exceptionExpected)
    {
        var request = new CreateTaskRequest("Test name", appointeeId);

        await Assert.ThrowsAsync(
                exceptionExpected,
                async () => await _service.CreateAsync(_admin.Id, _projectWithOpenTasks.Id, request));
    }

    [Fact]
    public async Task Create_ProjectDoesntExist_ThrowsError()
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.CreateAsync(_admin.Id, _nonExistingEntityId, _dummyCreateRequest));

    [Fact]
    public async Task Create_ValidRequest_CreatesTask()
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.CreateAsync(_admin.Id, _projectWithOpenTasks.Id, _dummyCreateRequest));

        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_ProjectDoenstExist_ThrowsError()
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.DeleteAsync(_nonExistingEntityId));

    [Fact]
    public async Task Delete_ValidRequest_ProjectDeleted()
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.DeleteAsync(_projectWithOpenTasks.Id));

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(_invalidUsersArguments))]
    public async Task GetAllForProject_InvalidRequester_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.GetAllForProjectAsync(requesterId, _projectWithOpenTasks.Id));

    [Theory]
    [MemberData(nameof(_validUserArguments))]
    public async Task GetAllForProject_ValidRequester_ReturnsProjectTasks(int requesterId)
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.GetAllForProjectAsync(requesterId, _projectWithOpenTasks.Id));

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(_invalidUsersArguments))]
    public async Task GetById_InvalidUser_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.GetByIdAsync(requesterId, _adminAssignedTask.Id));

    [Theory]
    [MemberData(nameof(_validUserArguments))]
    public async Task GetById_TaskDoesntExist_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.GetByIdAsync(requesterId, _nonExistingEntityId));

    [Theory]
    [MemberData(nameof(_validTaskRequesterArguments))]
    public async Task GetById_ValidRequest_ReturnsTask(int requesterId)
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.GetByIdAsync(requesterId, _adminAssignedTask.Id));

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(_invalidUsersArguments))]
    public async Task Update_InvalidRequester_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.UpdateAsync(requesterId, _adminAssignedTask.Id, _dummyUpdateRequest));

    [Theory]
    [MemberData(nameof(_validTaskRequesterArguments))]
    public async Task Update_TaskDoesntExist_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.UpdateAsync(requesterId, _nonExistingEntityId, _dummyUpdateRequest));

    [Theory]
    [MemberData(nameof(_validTaskRequesterArguments))]
    public async Task Update_ValidRequest_DoesNotThrowError(int requesterId)
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.UpdateAsync(requesterId, _adminAssignedTask.Id, _dummyUpdateRequest));

        Assert.Null(result);
    }
}
