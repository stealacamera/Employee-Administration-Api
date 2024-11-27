using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

public class Task : Entity
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(350)]
    public string? Description { get; set; }

    [Required]
    [DefaultValue(false)]
    public bool IsCompleted { get; set; } = false;


    [Required]
    public int ProjectId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Project Project { get; set; } = null!;


    [Required]
    public int AppointeeEmployeeId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User AppointeeEmployee { get; set; } = null!;


    [Required]
    public int AppointerUserId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User AppointerUser { get; set; } = null!;
}
