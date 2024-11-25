namespace EmployeeAdministration.Infrastructure.Options;

internal class JwtOptions
{
    public static string SectionName = "Jwt";

    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string SecretKey { get; init; } = null!;

    public double TokenExpiration_Minutes { get; init; }
    public double RefreshTokenExpiration_Minutes { get; init; }
}