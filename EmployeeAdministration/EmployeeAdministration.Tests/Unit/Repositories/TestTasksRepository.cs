using EmployeeAdministration.Infrastructure.Repositories;

namespace EmployeeAdministration.Tests.Unit.Repositories;

public class TestTasksRepository : BaseTestRepository
{
    private TasksRepository _repository = null!;

    public static readonly IEnumerable<object[]> 
        _userHasOpenTasksArguments = new List<object[]>
    {
        new object[] { _employee.Id, _openProject.Id },
        new object[] { _employee.Id, null },
    },
        _userHasNoOpenTasksArguments = new List<object[]>
    {
        new object[] { _employee.Id, _finishedProject.Id },
        new object[] { _deletedEmployee.Id, null },
    },
        _getProjectTasksArguments = new List<object[]>
    {
        new object[] { _openProject.Id, new[] { _openTask } },
        new object[] { _finishedProject.Id, new[] { _finishedTask } },
    },
        _userHasNoTasksArguments = new List<object[]>
    {
        new object[] { _employee.Id, _pausedProject.Id },
        new object[] { _deletedEmployee.Id, null },
    },
        _userHasTasksArguments = new List<object[]>
    {
        new object[] { _employee.Id, _openProject.Id, new[] { _openTask } },
        new object[] { _employee.Id, null, new[] { _openTask, _finishedTask } },
    };

    [Fact]
    public async Task DoesProjectHaveOpenTasks_NoOpenTasks_ReturnsFalse()
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        Assert.False(await _repository.DoesProjectHaveOpenTasksAsync(_finishedProject.Id));
    }

    [Fact]
    public async Task DoesProjectHaveOpenTasks_HasOpenTasks_ReturnsTrue()
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        Assert.True(await _repository.DoesProjectHaveOpenTasksAsync(_openProject.Id));
    }

    [Fact]
    public async Task DoesProjectHaveOpenTasks_NoTasks_ReturnsFalse()
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        Assert.False(await _repository.DoesProjectHaveOpenTasksAsync(_pausedProject.Id));
    }

    [Theory]
    [MemberData(nameof(_userHasNoOpenTasksArguments))]
    public async Task DoesUserHaveOpenTasks_NoOpenTasks_ReturnsFalse(int userId, int? projectId)
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        Assert.False(await _repository.DoesUserHaveOpenTasksAsync(userId, projectId: projectId));
    }

    [Theory]
    [MemberData(nameof(_userHasOpenTasksArguments))]
    public async Task DoesUserHaveOpenTasks_HasOpenTasks_ReturnsTrue(int userId, int? projectId)
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        Assert.True(await _repository.DoesUserHaveOpenTasksAsync(userId, projectId));
    }

    [Fact]
    public async Task GetAllForProject_NoTasks_ReturnsEmptyList()
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        Assert.Empty(await _repository.GetAllForProjectAsync(_pausedProject.Id));
    }

    [Theory]
    [MemberData(nameof(_getProjectTasksArguments))]
    public async Task GetAllForProject_HasTasks_ReturnsTasksList(int projectId, Domain.Entities.Task[] expectedResult)
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        var result = await _repository.GetAllForProjectAsync(projectId);
        var resultIds = result.Select(e => e.Id).ToArray();

        Assert.Equal(expectedResult.Length, result.Count());

        foreach (var item in expectedResult)
            Assert.Contains(item.Id, resultIds);
    }

    [Theory]
    [MemberData(nameof(_userHasNoTasksArguments))]
    public async Task GetAllForUser_NoTasks_ReturnsEmptyList(int userId, int? projectId)
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        Assert.Empty(await _repository.GetAllForUserAsync(userId, projectId));
    }

    [Theory]
    [MemberData(nameof(_userHasTasksArguments))]
    public async Task GetAllForUser_HasTasks_ReturnsTasks(int userId, int? projectId, Domain.Entities.Task[] expectedResult)
    {
        using var context = CreateContext();
        _repository = new TasksRepository(context);

        var result = await _repository.GetAllForUserAsync(userId, projectId);
        var resultIds = result.Select(e => e.Id).ToArray();

        Assert.Equal(expectedResult.Length, result.Count());

        foreach (var item in expectedResult)
            Assert.Contains(item.Id, resultIds);
    }
}
