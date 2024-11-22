using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAdministration.Domain.Entities;

[PrimaryKey(nameof(ProjectId), nameof(EmployeeId))]
public class ProjectMember : BaseEntity
{
    [ForeignKey(nameof(Project))]
    public int ProjectId { get; set; }
    public Project Project { get; set; }


    [ForeignKey(nameof(Employee))]
    public int EmployeeId { get; set; }
    public User Employee {  get; set; }
}