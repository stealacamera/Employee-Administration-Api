using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

public class Task : Entity
{
    [Required]
    public Project Project { get; set; }

    [Required]
    public User AppointeeEmployee { get; set; }

    [Required]
    public User AppointerEmployee { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Name { get; set; } = null!;

    [StringLength(300)]
    public string? Description { get; set; }

    [Required]
    public bool IsCompleted { get; set; } = false;
}
