using System.ComponentModel.DataAnnotations;

namespace EmployeeAdministration.Application.Common.DTOs;

public record Tokens(
    [Required] string JwtToken, 
    [Required] string RefreshToken);

public record RefreshTokensRequest(
    [Required, Range(1, int.MaxValue)] int RequesterId,
    [Required] Tokens Tokens);