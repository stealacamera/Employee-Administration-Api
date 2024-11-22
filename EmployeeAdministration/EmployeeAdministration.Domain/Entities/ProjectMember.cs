using Microsoft.EntityFrameworkCore;

namespace EmployeeAdministration.Domain.Entities;

[PrimaryKey(nameof(ProjectId), nameof(EmployeeId))]
public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public int EmployeeId { get; set; }
}