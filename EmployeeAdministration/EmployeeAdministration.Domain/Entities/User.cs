using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

public class User : IdentityUser<int>
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Surname { get; set; } = null!;


    [StringLength(300)]
    public string? ProfilePictureName { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual IList<IdentityUserRole<int>> Roles { get; set; } = [];
}
