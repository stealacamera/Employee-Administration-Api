using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Services;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestProjectsService
{
    private readonly ProjectsService _service;

    [Theory]
    // user doesnt exist
    // user del_at != null
    // user is not employee
    public async Task CreateWithMember_MemberIsInvalid_MemberNotCreated(int userId)
    {
        var request = new CreateProjectRequest();
        var result = await _service.CreateAsync(request);

        Assert.NotNull(_dbContext.Projects.GetById(result.Id);
        Assert.Null(_dbContext.ProjectMembers.GetById([result.Id, userId]));
    }

    [Fact]
    public async Task CreateWithMember_MemberIsValid_MemberCreated()
    {
        var request = new CreateProjectRequest();
        var result = await _service.CreateAsync(request);

        Assert.NotNull(_dbContext.Projects.GetById(result.Id));
        Assert.NotNull(_dbContext.ProjectMembers.GetById([result.Id, userId]));
    }

    [Theory]
    // project doesnt exist
    // proejct has open tasks
    public async Task Delete_ProjectIsInvalid_ThrowsError(int projectId, Type exceptionExpected)
    {
        await Assert.ThrowsAsync(
            exceptionExpected,
            async () => await _service.DeleteAsync(projectId));
    }

    [Fact]
    public async Task Delete_ProjectIsValid_TasksAndMembersDeleted()
    {
        await _service.DeleteAsync(projectId);

        Assert.Null(_dbContext.Projects.GetById(result.Id));
        Assert.False(await _dbContext.ProjectMembers.Where(e => e.ProjectId == projectId).AnyAsync());
        Assert.False(await _dbContext.Tasks.Where(e => e.ProjectId == projectId).AnyAsync());
    }

    [Theory]
    // user doesnt exist
    // user.del_at != null
    // user is empl + not project member
    public async Task GetById_InvalidRequester_ThrowsError(int userId, int projectId, Type exceptionExpected)
    {
        await Assert.ThrowsAsync(
            exceptionExpected,
            async () => await _service.GetByIdAsync(userId, projectId));
    }
}
