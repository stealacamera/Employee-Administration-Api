using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

public class Project : Entity
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(400)]
    public string? Description { get; set; }


    [Required]
    public sbyte StatusId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public ProjectStatus Status { get; set; } = null!;
}