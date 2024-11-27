using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

[PrimaryKey(nameof(ProjectId), nameof(EmployeeId))]
public class ProjectMember : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }
    
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Project Project { get; set; } = null!;


    [Required]
    public int EmployeeId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User Employee { get; set; } = null!;
}