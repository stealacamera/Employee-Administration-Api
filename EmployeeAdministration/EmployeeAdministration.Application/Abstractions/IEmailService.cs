namespace EmployeeAdministration.Application.Abstractions;

public interface IEmailService
{
    Task SendNewTaskAssignedEmailAsync(string appointeeEmail, string projectName, string taskName, string? taskDescription = null);
}
