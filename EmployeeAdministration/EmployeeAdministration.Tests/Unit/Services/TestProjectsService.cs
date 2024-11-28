using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestProjectsService : BaseTestService
{
    private readonly ProjectsService _service;

    public TestProjectsService() : base()
        => _service = new(_mockWorkUnit);

    [Theory]
    [InlineData(_nonExistingEntityId)]
    [InlineData(_deletedUser.Id)]
    [InlineData(_admin.Id)]
    public async Task Create_MemberIsInvalid_MemberNotCreated(int userId)
    {
        var request = new CreateProjectRequest("Test project", EmployeeIds: [userId]);
        var result = await _service.CreateAsync(request);

        Assert.Empty(result.Members);
    }

    [Fact]
    public async Task Create_MemberIsValid_MemberCreated()
    {
        var request = new CreateProjectRequest("Test project", EmployeeIds: [_nonMemberEmployee.Id]);
        var result = await _service.CreateAsync(request);

        Assert.Equal(1, result.Members.Count);
        Assert.Equal(_nonMemberEmployee.Id, result.Members[0].Id);
    }

    [Theory]
    [InlineData(_nonExistingEntityId, EntityNotFoundException)]
    [InlineData(_projectWithOpenTasks.Id, UncompletedTasksAssignedToEntityException)]
    public async Task Delete_ProjectIsInvalid_ThrowsError(int projectId, Type exceptionExpected)
        => await Assert.ThrowsAsync(
                exceptionExpected,
                async () => await _service.DeleteAsync(projectId));

    [Fact]
    public async Task Delete_ProjectIsValid_DoesNotThrowError()
    {
        var result = await Record.ExceptionAsync(async () => 
            await _service.DeleteAsync(_projectWithCompletedTasks.Id));

        Assert.Null(result);
    }

    [Theory]
    [InlineData(_nonExistingEntityId, EntityNotFoundException)]
    [InlineData(_deletedUser.Id, EntityNotFoundException)]
    [InlineData(_nonMemberEmployee.Id, UnauthorizedException)]
    public async Task GetById_InvalidRequester_ThrowsError(int userId, Type exceptionExpected)
        => await Assert.ThrowsAsync(
                exceptionExpected,
                async () => await _service.GetByIdAsync(_projectWithOpenTasks.Id, userId));

    [Fact]
    public async Task GetById_NonexistentProject_ThrowsError()
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.GetByIdAsync(_projectWithCompletedTasks.Id));

    [Fact]
    public async Task GetById_ValidRequest_DoesNotThrowError()
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.GetByIdAsync(_projectWithCompletedTasks.Id));

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_EmptyUpdate_ThrowsError()
        => await Assert.ThrowsAsync<ValidationException>(
                async () => await _service.UpdateAsync(_projectWithOpenTasks.Id, new UpdateProjectRequest());

    [Fact]
    public async Task Update_ValidUpdate_DoesNotThrowError()
    {
        var result = await Record.ExceptionAsync(async () => await _service.UpdateAsync(
                                                            _projectWithCompletedTasks.Id, 
                                                            new UpdateProjectRequest("Test name")));
        
        Assert.Null(result);
    }
}
