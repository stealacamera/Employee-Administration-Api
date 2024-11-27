using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Services;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestProjectMembersService
{
    private readonly ProjectMembersService _projectMembersService;

    [Fact]
    public async Task AddEmployeeToProject_ProjectDoesntExist_ThrowsError()
    {
        await Assert.ThrowsAsync(
            typeof(EntityNotFoundException),
            async () => await _projectMembersService.AddEmployeeToProjectAsync(employeeId, projectId));
    }

    [Theory]
    // user doesnt exist
    // user.del_at != null
    // user is not employee
    // user is already member
    public async Task AddEmployeeToProject_UserIsInvalid_ThrowsError(
        int employeeId, 
        int projectId, 
        Type exceptionTypeExpected)
    {
        await Assert.ThrowsAsync(
            exceptionTypeExpected, 
            async () => await _projectMembersService.AddEmployeeToProjectAsync(employeeId, projectId));
    }

    [Fact]
    public async Task AddEmployeeToProject_ReturnsProjectMember()
    {
        var result = await _projectMembersService.AddEmployeeToProjectAsync(employeeId, projectId);

        Assert.Equal(employeeId, result.Employee.Id);
        Assert.Equal(projectId, result.Project.Id);
    }

    [Fact]
    public async Task RemoveEmployeeFromProject_ProjectDoesntExist_ThrowsError()
    {
        await Assert.ThrowsAsync(
            typeof(EntityNotFoundException),
            async () => await _projectMembersService.RemoveEmployeeFromProjectAsync(employeeId, projectId));
    }

    [Theory]
    // user doesnt exist
    // user.del_at != null
    // user is not member
    // user has open tasks
    public async Task RemoveEmployeeFromProject_UserIsInvalid_ThrowsError(
        int employeeId,
        int projectId,
        Type exceptionTypeExpected)
    {
        await Assert.ThrowsAsync(
            exceptionTypeExpected,
            async () => await _projectMembersService.AddEmployeeToProjectAsync(employeeId, projectId));
    }

    [Fact]
    public async Task RemoveEmployeeFromProject_RemovesProjectMember()
    {
        var result = await _projectMembersService.AddEmployeeToProjectAsync(employeeId, projectId);
        Assert.Null(_dbContext.ProjectMembers.GetById([projectId, employeeId]));
    }
}
