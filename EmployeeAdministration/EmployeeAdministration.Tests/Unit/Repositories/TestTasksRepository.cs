using EmployeeAdministration.Infrastructure.Repositories;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestTasksRepository
{
    private readonly TasksRepository _tasksRepository;

    private int projectId;

    [Fact]
    public async Task DoesProjectHaveOpenTasks_NoOpenTasks_ReturnsFalse()
    {
        var result = await _tasksRepository.DoesProjectHaveOpenTasksAsync(projectId);
        Assert.False(result);
    }

    [Fact]
    public async Task DoesProjectHaveOpenTasks_HasOpenTasks_ReturnsTrue()
    {
        var result = await _tasksRepository.DoesProjectHaveOpenTasksAsync(projectId);
        Assert.True(result);
    }

    [Fact]
    public async Task DoesProjectHaveOpenTasks_NoTasks_ReturnsFalse()
    {
        var result = await _tasksRepository.DoesProjectHaveOpenTasksAsync(projectId);
        Assert.False(result);
    }

    [Theory]
    public async Task DoesUserHaveOpenTasks_NoOpenTasks_ReturnsFalse(int userId, int? projectId)
    {
        var result = await _tasksRepository.DoesUserHaveOpenTasksAsync(userId, projectId);
        Assert.False(result);
    }

    [Theory]
    public async Task DoesUserHaveOpenTasks_HasOpenTasks_ReturnsTrue(int userId, int? projectId)
    {
        var result = await _tasksRepository.DoesUserHaveOpenTasksAsync(userId, projectId);
        Assert.True(result);
    }

    [Theory]
    public async Task DoesUserHaveOpenTasks_NoTasks_ReturnsFalse(int userId, int? projectId)
    {
        var result = await _tasksRepository.DoesUserHaveOpenTasksAsync(userId, projectId);
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllForProject_NoTasks_ReturnsEmptyList()
    {
        var result = await _tasksRepository.GetAllForProjectAsync(projectId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllForProject_HasTasks_ReturnsEmptyList()
    {
        var expectedResult = new List<Domain.Entities.Task>();
        var result = await _tasksRepository.GetAllForProjectAsync(projectId);
        
        Assert.Equal(expectedResult.Count, result.Count());
        Assert.Equal(expectedResult.Select(e => e.Id).ToArray(), result.Select(e => e.Id).ToArray());
    }

    [Theory]
    public async Task GetAllForUser_NoTasks_ReturnsEmptyList(int userId, int? projectId)
    {
        var result = await _tasksRepository.GetAllForUserAsync(userId, projectId);
        Assert.Empty(result);
    }

    [Theory]
    public async Task GetAllForUser_HasTasks_ReturnsEmptyList(int userId, int? projectId)
    {
        var result = await _tasksRepository.GetAllForUserAsync(userId, projectId);

        Assert.Equal(expectedResult.Count, result.Count());
        Assert.Equal(expectedResult.Select(e => e.Id).ToArray(), result.Select(e => e.Id).ToArray());
    }
}
