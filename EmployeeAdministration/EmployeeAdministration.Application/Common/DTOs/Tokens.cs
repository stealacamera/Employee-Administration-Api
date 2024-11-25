using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Application.Common.DTOs;

public record Tokens(
    [Required] string JwtToken, 
    [Required] string RefreshToken);