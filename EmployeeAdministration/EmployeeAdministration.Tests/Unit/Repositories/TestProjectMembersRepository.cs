using EmployeeAdministration.Infrastructure.Repositories;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestProjectMembersRepository : BaseTestRepository
{
    private readonly ProjectMembersRepository _repository;

    public TestProjectMembersRepository() : base()
    {
        using var context = CreateContext();
        _repository = new ProjectMembersRepository(context);
    }

    [Fact]
    public async Task GetAllForProject_NoMemberships_ReturnsEmptyList()
         => Assert.Empty(await _repository.GetAllForProjectAsync(_emptyProject.Id));

    [Fact]
    public async Task GetAllForProject_HasMemberships_ReturnsMemberships()
    {
        var expectedResult = new[] { _openProjectMember };
        var result = await _repository.GetAllForProjectAsync(_openProject.Id);

        Assert.Equal(expectedResult.Length, result.Count());
        
        Assert.Equal(
            expectedResult.Select(e => (e.ProjectId, e.EmployeeId)).ToArray(), 
            result.Select(e => (e.ProjectId, e.EmployeeId)).ToArray());
    }

    [Fact]
    public async Task GetAllForUser_NoMemberships_ReturnsEmptyList()
        => Assert.Empty(await _repository.GetAllForUserAsync(_deletedEmployee.Id));

    [Fact]
    public async Task GetAllForUser_HasMemberships_ReturnsMemberships()
    {
        var expectedResult = new[] { _openProjectMember, _finishedProjectMember, _pausedProjectMember};
        var result = await _repository.GetAllForUserAsync(_employee.Id);

        Assert.Equal(expectedResult.Length, result.Count());

        Assert.Equal(
            expectedResult.Select(e => (e.ProjectId, e.EmployeeId)).ToArray(),
            result.Select(e => (e.ProjectId, e.EmployeeId)).ToArray());
    }
}
