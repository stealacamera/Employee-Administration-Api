using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Domain.Entities;

public class User : IdentityUser<int>
{
    [StringLength(300)]
    public string? ProfilePicture { get; set; }

    public DateTime? DeletedAt { get; set; }
}
