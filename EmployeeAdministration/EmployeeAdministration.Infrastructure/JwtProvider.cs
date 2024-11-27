using EmployeeAdministration.Application.Abstractions;
using EmployeeAdministration.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeAdministration.Infrastructure;

internal sealed class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> options)
        => _options = options.Value;

    public string GenerateRefreshToken()
    {
        var randNr = new byte[64];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randNr);
            return Convert.ToBase64String(randNr);
        }
    }

    public string GenerateToken(int userId, string userEmail)
    {
        // Create claim for user
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, userEmail)
        };

        // Create credentials
        var signingCredetials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            null,
            DateTime.UtcNow.AddMinutes(_options.TokenExpiration_Minutes),
            signingCredetials);

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;
    }
}
