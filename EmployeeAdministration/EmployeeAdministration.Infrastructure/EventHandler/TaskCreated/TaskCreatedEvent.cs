namespace EmployeeAdministration.Infrastructure.EventHandler.TaskCreated;

internal record TaskCreatedEvent(
    int TaskId,
    string TaskName,
    int ProjectId,
    string ProjectName,
    int AppointeeId,
    string AppointeeEmail,
    string? TaskDescription = null);