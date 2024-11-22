﻿using Microsoft.AspNetCore.Identity;

namespace EmployeeAdministration.Domain.Entities;

public class User : IdentityUser<int>
{
    public string? ProfilePicture { get; set; }
    public DateTime? DeletedAt { get; set; }
}
