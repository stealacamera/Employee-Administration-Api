﻿using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAdministration.Infrastructure;

internal class WorkUnit : IWorkUnit
{
    private readonly AppDbContext _dbContext;

    public WorkUnit(IServiceProvider serviceProvider)
        => _dbContext = serviceProvider.GetRequiredService<AppDbContext>();

    public async Task SaveChangesAsync()
        => await _dbContext.SaveChangesAsync();

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _dbContext.Database.BeginTransactionAsync();

    private IProjectsRepository _projectsRepository = null!;
    public IProjectsRepository ProjectsRepository
    {
        get
        {
            _projectsRepository ??= new ProjectsRepository(_dbContext);
            return _projectsRepository;
        }
    }

    private IProjectMembersRepository _projectMembersRepository = null!;
    public IProjectMembersRepository ProjectMembersRepository
    {
        get
        {
            _projectMembersRepository ??= new ProjectMembersRepository(_dbContext);
            return _projectMembersRepository;
        }
    }
    
    private ITasksRepository _tasksRepository = null!;
    public ITasksRepository TasksRepository
    {
        get
        {
            _tasksRepository ??= new TasksRepository(_dbContext);
            return _tasksRepository;
        }
    }
}
