using Azure.Core;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Services;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestTasksService : BaseTestService
{
    private readonly TasksService _service;
    private readonly UpdateTaskRequest _dummyUpdateRequest = new("Test name");
    private readonly CreateTaskRequest _dummyCreateRequest = new(_memberEmployee.Id, "Test name");

    public TestTasksService() : base()
        => _service = new TasksService(_mockWorkUnit);

    [Theory]
    [InlineData(_nonExistingEntityId)]
    [InlineData(_deletedUser.Id)]
    [InlineData(_nonMemberEmployee.Id)]
    public async Task Create_AppointerIsInvalid_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.CreateAsync(requesterId, _projectWithOpenTasks.Id, _dummyCreateRequest));

    [Theory]
    [InlineData(_nonExistingEntityId, EntityNotFoundException)]
    [InlineData(_deletedUser.Id, EntityNotFoundException)]
    [InlineData(_admin.Id, NonEmployeeUserException)]
    [InlineData(_nonMemberEmployee.Id, NotAProjectMemberException)]
    public async Task Create_AppointeeIsInvalid_ThrowsError(int appointeeId, Type exceptionExpected)
    {
        var request = new CreateTaskRequest(appointeeId, "Test name");

        await Assert.ThrowsAsync(
                exceptionExpected,
                async () => await _service.CreateAsync(_admin.Id, projectId, request));
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
    [InlineData(_nonExistingEntityId, EntityNotFoundException)]
    [InlineData(_deletedUser.Id, EntityNotFoundException)]
    [InlineData(_nonMemberEmployee, NotAProjectMemberException)]
    public async Task GetAllForProject_InvalidRequester_ThrowsError(int requesterId, Type exceptionExpected)
        => await Assert.ThrowsAsync(
                exceptionExpected,
                async () => await _service.GetAllForProjectAsync(requesterId, _projectWithOpenTasks.Id));

    [Theory]
    [InlineData(_admin.Id)]
    [InlineData(_memberEmployee.Id)]
    public async Task GetAllForProject_ValidRequester_ReturnsProjectTasks(int requesterId)
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.GetAllForProjectAsync(requesterId, _projectWithOpenTasks.Id));

        Assert.Null(result);
    }

    [Theory]
    [InlineData(_nonExistingEntityId)]
    [InlineData(_deletedUser.Id)]
    [InlineData(_nonMemberEmployee.Id)]
    public async Task GetById_InvalidUser_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(
                async () => _service.GetByIdAsync(requesterId, _adminAssignedTask.Id));

    [Theory]
    [InlineData(_admin.Id)]
    [InlineData(_memberEmployee.Id)]
    public async Task GetById_TaskDoesntExist_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.GetByIdAsync(requesterId, _nonExistingEntityId));

    [Theory]
    [InlineData(_admin.Id)]
    [InlineData(_adminAssignedTask.AppointeeEmployeeId)]
    public async Task GetById_ValidRequest_ReturnsTask(int requesterId)
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.GetByIdAsync(requesterId, _adminAssignedTask.Id));

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(_nonExistingEntityId)]
    [InlineData(_deletedUser.Id)]
    [InlineData(_nonMemberEmployee.Id)]
    public async Task Update_InvalidRequester_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.UpdateAsync(requesterId, _adminAssignedTask.Id, _dummyUpdateRequest);

    [Theory]
    [InlineData(_admin.Id)]
    [InlineData(_adminAssignedTask.AppointeeEmployeeId)]
    public async Task Update_TaskDoesntExist_ThrowsError(int requesterId)
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.UpdateAsync(requesterId, _nonExistingEntityId, _dummyUpdateRequest);

    [Theory]
    [InlineData(_admin.Id)]
    [InlineData(_adminAssignedTask.AppointeeEmployeeId)]
    public async Task Update_ValidRequest_DoesNotThrowError(int requesterId)
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.UpdateAsync(requesterId, _adminAssignedTask.Id, _dummyUpdateRequest));

        Assert.Null(result);
    }
}
