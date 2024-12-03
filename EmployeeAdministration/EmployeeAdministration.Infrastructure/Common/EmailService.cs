using EmployeeAdministration.Application.Abstractions;
using FluentEmail.Core;
using Task = System.Threading.Tasks.Task;

namespace EmployeeAdministration.Infrastructure.Common;

internal class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;

    public EmailService(IFluentEmail fluentEmail)
    {
        _fluentEmail = fluentEmail;
    }

    public async Task SendNewTaskAssignedEmailAsync(string appointeeEmail, string projectName, string taskName, string? taskDescription = null)
    {
        var message = $"A new task was assigned to you for \"{projectName}\" project :\n\n" +
            $"Task: {taskName}\n{taskDescription ?? ""}";

        await SendEmailAsync(appointeeEmail, $"New task assigned for {projectName}", message);
    }

    private async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _fluentEmail.To(email)
                          .Subject(subject)
                          .Body(htmlMessage, isHtml: true)
                          .SendAsync();
    }
}
