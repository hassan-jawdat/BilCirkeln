using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BilCirkeln.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace BilCirkeln.Api.Services;

public interface ITokenService
{
    string CreateToken(ApplicationUser user, string role);
}

// Creates a JWT (JSON Web Token). The token is a signed "ID card" that the
// frontend sends with every request so the API knows who the user is.
public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(ApplicationUser user, string role)
    {
        // Claims = facts about the user that are stored inside the token.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role)
        };

        // The secret key comes from user secrets, never from the code.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
