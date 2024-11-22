using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Project : Entity
{
    [Required]
    public string Name { get; set; } = null!;
}