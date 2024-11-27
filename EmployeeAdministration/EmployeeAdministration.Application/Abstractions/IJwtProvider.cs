namespace EmployeeAdministration.Application.Abstractions;

public interface IJwtProvider
{
    string GenerateToken(int userId, string userEmail);
    string GenerateRefreshToken();
}