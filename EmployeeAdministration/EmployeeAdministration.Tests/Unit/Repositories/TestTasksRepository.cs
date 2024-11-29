using EmployeeAdministration.Infrastructure.Repositories;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestTasksRepository : BaseTestRepository
{
    private readonly TasksRepository _repository;

    public TestTasksRepository() : base()
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);
    }

    [Fact]
    public async Task DoesProjectHaveOpenTasks_NoOpenTasks_ReturnsFalse()
        => Assert.False(await _repository.DoesProjectHaveOpenTasksAsync(_finishedProject.Id));

    [Fact]
    public async Task DoesProjectHaveOpenTasks_HasOpenTasks_ReturnsTrue()
        => Assert.True(await _repository.DoesProjectHaveOpenTasksAsync(_openProject.Id));

    [Fact]
    public async Task DoesProjectHaveOpenTasks_NoTasks_ReturnsFalse()
        => Assert.False(await _repository.DoesProjectHaveOpenTasksAsync(_pausedProject.Id));

    public static readonly IEnumerable<object[]> _userHasOpenTasks = new List<object[]>
    {
        new object[] { _employee.Id, _openProject.Id },
        new object[] { _employee.Id, null },
    };

    [Theory]
    [MemberData(nameof(_userHasNoOpenTasks))]
    public async Task DoesUserHaveOpenTasks_NoOpenTasks_ReturnsFalse(int userId, int? projectId)
        => Assert.False(await _repository.DoesUserHaveOpenTasksAsync(userId, projectId: projectId));

    public static readonly IEnumerable<object[]> _userHasNoOpenTasks = new List<object[]>
    {
        new object[] { _employee.Id, _finishedProject.Id },
        new object[] { _deletedEmployee, null },
    };

    [Theory]
    [MemberData(nameof(_userHasOpenTasks))]
    public async Task DoesUserHaveOpenTasks_HasOpenTasks_ReturnsTrue(int userId, int? projectId)
        => Assert.True(await _repository.DoesUserHaveOpenTasksAsync(userId, projectId));

    [Fact]
    public async Task GetAllForProject_NoTasks_ReturnsEmptyList()
        => Assert.Empty(await _repository.GetAllForProjectAsync(_pausedProject.Id));

    public static readonly IEnumerable<object[]> _getProjectTasksArguments = new List<object[]>
    {
        new object[] { _openProject.Id, new[] { _openTask } },
        new object[] { _finishedProject.Id, new[] { _finishedTask } },
    };

    [Theory]
    [MemberData(nameof(_getProjectTasksArguments))]
    public async Task GetAllForProject_HasTasks_ReturnsTasksList(int projectId, Task[] expectedResult)
    {
        var result = await _repository.GetAllForProjectAsync(projectId);
        
        Assert.Equal(expectedResult.Length, result.Count());
        Assert.Equal(expectedResult.Select(e => e.Id).ToArray(), result.Select(e => e.Id).ToArray());
    }

    public static readonly IEnumerable<object[]> _getUserTasks_ReturnsEmpty_Arguments = new List<object[]>
    {
        new object[] { _employee.Id, _pausedProject.Id },
        new object[] { _deletedEmployee.Id, null },
    };

    [Theory]
    [MemberData(nameof(_getUserTasks_ReturnsEmpty_Arguments))]
    public async Task GetAllForUser_NoTasks_ReturnsEmptyList(int userId, int? projectId)
        => Assert.Empty(await _repository.GetAllForUserAsync(userId, projectId));

    public static readonly IEnumerable<object[]> _getUserTasks_ReturnsTasks_Arguments = new List<object[]>
    {
        new object[] { _employee.Id, _openProject.Id, new[] { _openTask } },
        new object[] { _employee.Id, null, new[] { _openTask, _finishedTask } },
    };

    [Theory]
    [MemberData(nameof(_getUserTasks_ReturnsTasks_Arguments))]
    public async Task GetAllForUser_HasTasks_ReturnsTasks(int userId, int? projectId, Task[] expectedResult)
    {
        var result = await _repository.GetAllForUserAsync(userId, projectId);

        Assert.Equal(expectedResult.Length, result.Count());
        Assert.Equal(expectedResult.Select(e => e.Id).ToArray(), result.Select(e => e.Id).ToArray());
    }
}
