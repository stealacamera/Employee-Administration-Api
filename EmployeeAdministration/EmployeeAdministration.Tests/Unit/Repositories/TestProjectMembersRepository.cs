using EmployeeAdministration.Infrastructure.Repositories;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestProjectMembersRepository : BaseTestRepository
{
    private ProjectMembersRepository _repository = null!;

    [Fact]
    public async Task GetAllForProject_NoMemberships_ReturnsEmptyList()
    {
        using var dbContext = CreateContext();
        _repository = new ProjectMembersRepository(dbContext);

        Assert.Empty(await _repository.GetAllForProjectAsync(_emptyProject.Id));
    }

    [Fact]
    public async Task GetAllForProject_HasMemberships_ReturnsMemberships()
    {
        using var dbContext = CreateContext();
        _repository = new ProjectMembersRepository(dbContext);

        var expectedResult = new[] { _openProjectMember };
        var result = await _repository.GetAllForProjectAsync(_openProject.Id);

        Assert.Equal(expectedResult.Length, result.Count());
        
        Assert.Equal(
            expectedResult.Select(e => (e.ProjectId, e.EmployeeId)).ToArray(), 
            result.Select(e => (e.ProjectId, e.EmployeeId)).ToArray());
    }

    [Fact]
    public async Task GetAllForUser_NoMemberships_ReturnsEmptyList()
    {
        using var dbContext = CreateContext();
        _repository = new ProjectMembersRepository(dbContext);

        Assert.Empty(await _repository.GetAllForUserAsync(_deletedEmployee.Id));
    }

    [Fact]
    public async Task GetAllForUser_HasMemberships_ReturnsMemberships()
    {
        using var dbContext = CreateContext();
        _repository = new ProjectMembersRepository(dbContext);

        var expectedResult = new[] { _openProjectMember, _finishedProjectMember, _pausedProjectMember};
        var result = await _repository.GetAllForUserAsync(_employee.Id);
        var resultIds = result.Select(e => (e.ProjectId, e.EmployeeId)).ToArray();

        Assert.Equal(expectedResult.Length, result.Count());

        foreach (var item in expectedResult)
            Assert.Contains((item.ProjectId, item.EmployeeId), resultIds);
    }
}
