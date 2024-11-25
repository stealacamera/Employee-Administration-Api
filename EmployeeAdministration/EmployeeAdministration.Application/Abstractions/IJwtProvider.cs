namespace EmployeeAdministration.Application.Abstractions;

public interface IJwtProvider
{
    int ExtractIdFromToken(string token);

    string GenerateToken(int userId, string userEmail);
    string GenerateRefreshToken();
}