﻿using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Services;
using EmployeeAdministration.Application.Common.DTOs;
using EmployeeAdministration.Application.Common.Exceptions;
using EmployeeAdministration.Application.Common.Exceptions.General;
using EmployeeAdministration.Domain.Enums;
using EmployeeAdministration.Infrastructure.EventHandler.TaskCreated;
using Task = EmployeeAdministration.Application.Common.DTOs.Task;

namespace EmployeeAdministration.Infrastructure.Services;

internal class TasksService : BaseService, ITasksService
{
    private readonly IEventBus _eventBus;

    public TasksService(IWorkUnit workUnit, IEventBus eventBus) : base(workUnit)
        => _eventBus = eventBus;

    public async Task<Task> CreateAsync(int requesterId, int projectId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        bool isTaskSelfAssigned = requesterId == request.AppointeeId;
        var requester = await ValidateRequesterIsAdminOrInProjectAsync(requesterId, projectId, cancellationToken);
        var project = await _workUnit.ProjectsRepository.GetByIdAsync(projectId, cancellationToken);

        if (project == null)
            throw new EntityNotFoundException(nameof(Project));

        var appointee = await ValidateAppointeeToCreateTaskAsync(request.AppointeeId ?? requesterId, projectId, isTaskSelfAssigned, cancellationToken);

        // Add new task
        var newTask = new Domain.Entities.Task
        {
            AppointeeEmployeeId = appointee.Id,
            AppointerUserId = requesterId,
            Name = request.Name,
            Description = request.Description,
            ProjectId = projectId,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
        };

        await _workUnit.TasksRepository.AddAsync(newTask, cancellationToken);
        await _workUnit.SaveChangesAsync();

        //await _eventBus.PublishAsync(
        //        new TaskCreatedEvent(
        //            newTask.Id, newTask.Name, 
        //            project.Id, project.Name, 
        //            appointee.Id, appointee.Email!, 
        //            newTask.Description));

        return new Task(
            newTask.Id,
            isTaskSelfAssigned ? null : new BriefUser(requester.Id, requester.Email, requester.FirstName, requester.Surname, null),
            new BriefUser(appointee.Id, appointee.Email, appointee.FirstName, appointee.Surname, null),
            newTask.Name, newTask.IsCompleted, newTask.CreatedAt, newTask.Description);
    }
    
    public async System.Threading.Tasks.Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _workUnit.TasksRepository
                                  .GetByIdAsync(id, cancellationToken);

        if (task == null)
            throw new EntityNotFoundException(nameof(Task));

        _workUnit.TasksRepository.Delete(task);
        await _workUnit.SaveChangesAsync();
    }

    public async Task<IList<Task>> GetAllForProjectAsync(int requesterId, int projectId, CancellationToken cancellationToken = default)
    {
        var requester = await ValidateRequesterIsAdminOrInProjectAsync(requesterId, projectId, cancellationToken);

        if (!await _workUnit.ProjectsRepository.DoesInstanceExistAsync(projectId, cancellationToken))
            throw new EntityNotFoundException(nameof(Project));

        return (await _workUnit.TasksRepository
                               .GetAllForProjectAsync(projectId, cancellationToken))
                               .Select(async e => await ConvertTaskToModelAsync(e, cancellationToken))
                               .Select(e => e.Result)
                               .ToList();
    }

    public async Task<Task> GetByIdAsync(int requesterId, int id, CancellationToken cancellationToken = default)
    {
        var task = await _workUnit.TasksRepository
                                  .GetByIdAsync(id, cancellationToken);
        
        if (task == null)
            throw new EntityNotFoundException(nameof(Task));

        await ValidateRequesterIsAdminOrTaskAppointeeAsync(requesterId, task, cancellationToken);
        return await ConvertTaskToModelAsync(task, cancellationToken);
    }

    public async Task<Task> UpdateAsync(int requesterId, int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request.IsCompleted == null && request.Name == null && request.Description == null)
            ValidationException.GenerateExceptionForEmptyRequest();

        var task = await _workUnit.TasksRepository
                                  .GetByIdAsync(id, cancellationToken);

        if (task == null)
            throw new EntityNotFoundException(nameof(Task));

        await ValidateRequesterIsAdminOrTaskAppointeeAsync(requesterId, task, cancellationToken);

        // Update task
        if (request.Name != null)
            task.Name = request.Name;
        if (request.Description != null)
            task.Description = request.Description;
        if (request.IsCompleted.HasValue)
            task.IsCompleted = request.IsCompleted.Value;

        task.UpdatedAt = DateTime.UtcNow;
        await _workUnit.SaveChangesAsync();

        return await ConvertTaskToModelAsync(task, cancellationToken);
    }


    // Helper functions
    private async Task<Domain.Entities.User> ValidateRequesterIsAdminOrInProjectAsync(
        int requesterId,
        int projectId,
        CancellationToken cancellationToken)
    {
        var requester = await _workUnit.UsersRepository
                                       .GetByIdAsync(requesterId, cancellationToken: cancellationToken);

        if (requester == null)
            throw new UnauthorizedException();

        bool isRequesterAdmin = await _workUnit.UserRolesRepository
                                               .IsUserInRoleAsync(requester.Id, Roles.Administrator, cancellationToken);

        if (!isRequesterAdmin && !await _workUnit.ProjectMembersRepository
                                                 .IsUserMemberAsync(requesterId, projectId, cancellationToken))
            throw new UnauthorizedException();

        return requester;
    }

    private async System.Threading.Tasks.Task ValidateRequesterIsAdminOrTaskAppointeeAsync(
        int requesterId,
        Domain.Entities.Task task,
        CancellationToken cancellationToken)
    {
        var requester = await _workUnit.UsersRepository
                                       .GetByIdAsync(requesterId, cancellationToken: cancellationToken);

        if (requester == null)
            throw new UnauthorizedException();

        bool isRequesterAdmin = await _workUnit.UserRolesRepository
                                               .IsUserInRoleAsync(requester.Id, Roles.Administrator, cancellationToken);

        if (!isRequesterAdmin && requester.Id != task.AppointeeEmployeeId)
            throw new UnauthorizedException();
    }

    private async Task<Domain.Entities.User> ValidateAppointeeToCreateTaskAsync(
        int appointeeId,
        int projectId,
        bool isTaskSelfAssigned,
        CancellationToken cancellationToken)
    {
        var appointee = await _workUnit.UsersRepository.GetByIdAsync(appointeeId, cancellationToken: cancellationToken);

        // Check appointee exists and is an employee in the project
        if (appointee == null)
            throw new EntityNotFoundException(nameof(User));

        var isUserEmployee = await _workUnit.UserRolesRepository
                                            .IsUserInRoleAsync(appointee.Id, Roles.Employee, cancellationToken);

        if (!isUserEmployee)
            throw new NonEmployeeUserException();
        else if (!isTaskSelfAssigned && !await _workUnit.ProjectMembersRepository
                                                        .IsUserMemberAsync(appointee.Id, projectId, cancellationToken))
            throw new NotAProjectMemberException();

        return appointee;
    }

    private async Task<Task> ConvertTaskToModelAsync(Domain.Entities.Task entity, CancellationToken cancellationToken)
    {
        bool isTaskSelfAssigned = entity.AppointeeEmployeeId == entity.AppointerUserId;
        BriefUser? appointer = null;

        if (!isTaskSelfAssigned)
        {
            var appointerDb = (await _workUnit.UsersRepository
                                              .GetByIdAsync(
                                                    entity.AppointerUserId, 
                                                    excludeDeletedUser: false, 
                                                    cancellationToken))!;

            appointer = new(
                appointerDb.Id, appointerDb.Email,
                appointerDb.FirstName, appointerDb.Surname,
                appointerDb.DeletedAt);
        }

        var appointeeDb = (await _workUnit.UsersRepository
                                          .GetByIdAsync(
                                                entity.AppointeeEmployeeId, 
                                                excludeDeletedUser: false, 
                                                cancellationToken: cancellationToken))!;

        var appointee = new BriefUser(
            appointeeDb.Id, appointeeDb.Email,
            appointeeDb.FirstName, appointeeDb.Surname,
            appointeeDb.DeletedAt);

        return new Task(
            entity.Id, 
            appointer, appointee, 
            entity.Name, entity.IsCompleted, 
            entity.CreatedAt, entity.Description);
    }
}
