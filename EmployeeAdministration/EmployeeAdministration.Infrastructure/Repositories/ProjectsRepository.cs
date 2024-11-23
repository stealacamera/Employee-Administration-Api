using EmployeeAdministration.Application.Abstractions.Repositories;
using EmployeeAdministration.Domain.Entities;

namespace EmployeeAdministration.Infrastructure.Repositories;

internal class ProjectsRepository : Repository<Project>, IProjectsRepository
{
    public ProjectsRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
