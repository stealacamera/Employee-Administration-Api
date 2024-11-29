using System.ComponentModel.DataAnnotations;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Services;
using Task = System.Threading.Tasks.Task;
using ValidationException = EmployeeAdministration.Application.Common.Exceptions.ValidationException;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestProjectsService : BaseTestService
{
    private readonly ProjectsService _service;

    public TestProjectsService() : base()
        => _service = new(_mockWorkUnit);

    public static readonly IEnumerable<object[]> _createProject_InvalidMember_Arguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId },
        new object[] { _deletedUser.Id },
        new object[] { _admin.Id },
    };

    [Theory]
    [MemberData(nameof(_createProject_InvalidMember_Arguments))]
    public async Task Create_MemberIsInvalid_MemberNotCreated(int userId)
    {
        var request = new CreateProjectRequest("Test project", EmployeeIds: [userId]);
        Assert.Empty((await _service.CreateAsync(request)).Members);
    }

    [Fact]
    public async Task Create_MemberIsValid_MemberCreated()
    {
        var result = await _service.CreateAsync(new("Test project", EmployeeIds: [_nonMemberEmployee.Id]));

        Assert.Single(result.Members);
        Assert.Equal(_nonMemberEmployee.Id, result.Members[0].Id);
    }

    public static readonly IEnumerable<object[]> _deleteProject_InvalidProject_Arguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId, typeof(EntityNotFoundException) },
        new object[] { _projectWithOpenTasks.Id, typeof(UncompletedTasksAssignedToEntityException) },
    };

    [Theory]
    [MemberData(nameof(_deleteProject_InvalidProject_Arguments))]
    public async Task Delete_ProjectIsInvalid_ThrowsError(int projectId, Type exceptionExpected)
        => await Assert.ThrowsAsync(
            exceptionExpected, async () => await _service.DeleteAsync(projectId));

    [Fact]
    public async Task Delete_ProjectIsValid_DoesNotThrowError()
    {
        var result = await Record.ExceptionAsync(async () => 
            await _service.DeleteAsync(_projectWithCompletedTasks.Id));

        Assert.Null(result);
    }

    public static readonly IEnumerable<object[]> _getAllForProject_InvalidRequester_Arguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId, typeof(EntityNotFoundException) },
        new object[] { _deletedUser.Id, typeof(EntityNotFoundException) },
        new object[] { _nonMemberEmployee.Id, typeof(UnauthorizedException) },
    };

    [Theory]
    [MemberData(nameof(_getAllForProject_InvalidRequester_Arguments))]
    public async Task GetById_InvalidRequester_ThrowsError(int userId, Type exceptionExpected)
        => await Assert.ThrowsAsync(
                exceptionExpected, async () => await _service.GetByIdAsync(_projectWithOpenTasks.Id, userId));

    [Fact]
    public async Task GetById_NonexistentProject_ThrowsError()
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.GetByIdAsync(_memberEmployee.Id, _nonExistingEntityId));

    [Fact]
    public async Task GetById_ValidRequest_DoesNotThrowError()
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.GetByIdAsync(_memberEmployee.Id, _projectWithCompletedTasks.Id));

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_EmptyUpdate_ThrowsError()
        => await Assert.ThrowsAsync<ValidationException>(
                async () => await _service.UpdateAsync(_projectWithOpenTasks.Id, new()));

    [Fact]
    public async Task Update_ValidUpdate_DoesNotThrowError()
    {
        var result = await Record.ExceptionAsync(
            async () => await _service.UpdateAsync(_projectWithCompletedTasks.Id, new("Test name")));
        
        Assert.Null(result);
    }
}
