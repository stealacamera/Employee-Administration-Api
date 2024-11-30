using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Services;
using NSubstitute;
using Task = System.Threading.Tasks.Task;

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

    public static readonly IEnumerable<object[]> _addEmployeeToProjectArguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId, typeof(EntityNotFoundException) },
        new object[] { _deletedEmployee.Id, typeof(EntityNotFoundException) },
        new object[] { _admin.Id, typeof(NonEmployeeUserException) },
        new object[] { _memberEmployee.Id, typeof(ExistingProjectMemberException) },
    };

    [Theory]
    [MemberData(nameof(_addEmployeeToProjectArguments))]
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
                async () => await _service.RemoveEmployeeFromProjectAsync(_nonMemberEmployee.Id, _nonExistingEntityId));

    public static readonly IEnumerable<object[]> _removeEmployeeFromProjectArguments = new List<object[]>
    {
        new object[] { _nonExistingEntityId, typeof(EntityNotFoundException) },
        new object[] { _deletedEmployee.Id, typeof(EntityNotFoundException) },
        new object[] { _nonMemberEmployee.Id, typeof(NotAProjectMemberException) },
        new object[] { _memberEmployee.Id, typeof(UncompletedTasksAssignedToEntityException) },
    };

    [Theory]
    [MemberData(nameof(_removeEmployeeFromProjectArguments))]
    public async Task RemoveEmployeeFromProject_UserIsInvalid_ThrowsError(int employeeId, Type exceptionTypeExpected)
        => await Assert.ThrowsAsync(
                exceptionTypeExpected,
                async () => await _service.RemoveEmployeeFromProjectAsync(employeeId, _projectWithOpenTasks.Id));

    [Fact]
    public async Task RemoveEmployeeFromProject_ValidRequest_RemovesMember()
    {
        var result = await Record.ExceptionAsync(async () => 
            await _service.RemoveEmployeeFromProjectAsync(_memberEmployee.Id, _projectWithCompletedTasks.Id));
        
        Assert.Null(result);
    }
}
