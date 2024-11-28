using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Services;
using EmployeeAdministration.Domain.Entities;
using NSubstitute;

namespace EmployeeAdministration.Tests.Unit.Services;

public class TestProjectMembersService : BaseTestService
{
    private readonly ProjectMembersService _service;
    
    public TestProjectMembersService() : base()
        => _service = new(_mockWorkUnit);

    [Fact]
    public async Task AddEmployeeToProject_ProjectDoesntExist_ThrowsError()
        => await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await _service.AddEmployeeToProjectAsync(_nonMemberEmployee.Id, 0));

    [Theory]
    [InlineData(_nonExistingEntityId, EntityNotFoundException)]
    [InlineData(_deletedUser.Id, EntityNotFoundException)]
    [InlineData(_admin.Id, NonEmployeeUserException)]
    [InlineData(_memberEmployee.Id, ExistingProjectMemberException)]
    public async Task AddEmployeeToProject_UserIsInvalid_ThrowsError(int employeeId, Type exceptionTypeExpected)
        => await Assert.ThrowsAsync(
                exceptionTypeExpected, 
                async () => await _service.AddEmployeeToProjectAsync(employeeId, _projectWithOpenTasks.Id));

    [Fact]
    public async Task AddEmployeeToProject_ValidRequest_ReturnsProjectMember()
    {
        var result = await _service.AddEmployeeToProjectAsync(_nonMemberEmployee.Id, _projectWithOpenTasks.Id);

        Assert.Equal(_nonMemberEmployee.Id, result.Employee.Id);
        Assert.Equal(_projectWithOpenTasks.Id, result.Project.Id);
    }

    [Fact]
    public async Task RemoveEmployeeFromProject_ProjectDoesntExist_ThrowsError()
        => await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _service.RemoveEmployeeFromProjectAsync(_nonMemberEmployee.Id, 0));

    [Theory]
    [InlineData(_nonExistingEntityId, EntityNotFoundException)]
    [InlineData(_deletedUser.Id, EntityNotFoundException)]
    [InlineData(_nonMemberEmployee.Id, EntityNotFoundException)]
    [InlineData(_memberEmployee.Id, UncompletedTasksAssignedToEntityException)]
    public async Task RemoveEmployeeFromProject_UserIsInvalid_ThrowsError(int employeeId, Type exceptionTypeExpected)
        => await Assert.ThrowsAsync(
                exceptionTypeExpected,
                async () => await _service.AddEmployeeToProjectAsync(employeeId, _projectWithOpenTasks.Id));

    [Fact]
    public async Task RemoveEmployeeFromProject_ValidRequest_RemovesMember()
    {
        _mockWorkUnit.TasksRepository
                     .DoesUserHaveOpenTasksAsync(_memberEmployee.Id, _projectWithOpenTasks.Id)
                     .Returns(false);

        var result = await Record.ExceptionAsync(async () => 
            await _service.AddEmployeeToProjectAsync(_memberEmployee.Id, _projectWithOpenTasks.Id));
        
        Assert.Null(result);
    }
}
