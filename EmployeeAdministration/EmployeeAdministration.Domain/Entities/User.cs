using Microsoft.AspNetCore.Identity;

namespace EmployeeAdministration.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = null!;
    public string Surname { get; set; } = null!;

    public string? ProfilePictureName { get; set; }
    public DateTime? DeletedAt { get; set; }

    public int RoleId { get; set; }
}
