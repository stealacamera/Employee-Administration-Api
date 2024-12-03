using EmployeeAdministration.Application.Abstractions;
using MassTransit;

namespace EmployeeAdministration.Infrastructure.EventHandler.TaskCreated;

internal class TaskCreatedEventConsumer : IConsumer<TaskCreatedEvent>
{
    private readonly IEmailService _emailService;

    public TaskCreatedEventConsumer(IEmailService emailService)
        => _emailService = emailService;

    public async Task Consume(ConsumeContext<TaskCreatedEvent> context)
        => await _emailService.SendNewTaskAssignedEmailAsync(
            context.Message.AppointeeEmail,
            context.Message.ProjectName,
            context.Message.TaskName,
            context.Message.TaskDescription);
}
