using EmployeeAdministration.Infrastructure.Repositories;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestProjectMembersRepository
{
    private readonly ProjectMembersRepository _projectMembersRepository;

    [Fact]
    public async Task GetAllForProject_NoMemberships_ReturnsEmptyList()
    {
        var result = await _projectMembersRepository.GetAllForProjectAsync(projectId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllForProject_HasMemberships_ReturnsMemberships()
    {
        var expectedResult = new List<Domain.Entities.ProjectMember>();
        var result = await _projectMembersRepository.GetAllForProjectAsync(projectId);

        Assert.Equal(expectedResult.Count, result.Count());
        
        Assert.Equal(
            expectedResult.Select(e => (e.ProjectId, e.EmployeeId)).ToArray(), 
            result.Select(e => (e.ProjectId, e.EmployeeId).ToArray());
    }

    [Fact]
    public async Task GetAllForUser_NoMemberships_ReturnsEmptyList()
    {
        var result = await _projectMembersRepository.GetAllForUserAsync(projectId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllForUser_HasMemberships_ReturnsMemberships()
    {
        var expectedResult = new List<Domain.Entities.ProjectMember>();
        var result = await _projectMembersRepository.GetAllForUserAsync(userId);

        Assert.Equal(expectedResult.Count, result.Count());

        Assert.Equal(
            expectedResult.Select(e => (e.ProjectId, e.EmployeeId)).ToArray(),
            result.Select(e => (e.ProjectId, e.EmployeeId).ToArray());
    }
}
