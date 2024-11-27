using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

public class ProjectStatus
{
    [Key]
    public sbyte Id { get; set; }
    
    [Required]
    [StringLength(40)]
    public string Name { get; set; } = null!;

    [Required]
    public sbyte Value { get; set; }
}