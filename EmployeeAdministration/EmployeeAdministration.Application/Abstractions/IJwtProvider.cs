using EmployeeAdministration.Domain.Entities;

namespace EmployeeAdministration.Application.Abstractions;

public interface IJwtProvider
{
    int ExtractIdFromToken(string token);
    void UpdateRefreshToken(User user);

    string GenerateToken(int userId, string userEmail);
    string GenerateRefreshToken();
}