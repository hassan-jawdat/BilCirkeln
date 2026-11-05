using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BilCirkeln.Api.Models;
using BilCirkeln.Api.Services;
using Microsoft.Extensions.Configuration;

namespace BilCirkeln.Tests;

public class TokenServiceTests
{
    private static TokenService CreateService()
    {
        // A fake configuration with a test key (never the real one).
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-at-least-32-characters-long",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();

        return new TokenService(config);
    }

    [Fact]
    public void CreateToken_ContainsUserIdEmailAndRole()
    {
        var user = new ApplicationUser { Id = "user-1", Email = "kund@test.se", FullName = "Kund Ett" };

        var tokenString = CreateService().CreateToken(user, Roles.Customer);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

        Assert.Equal("user-1", token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("kund@test.se", token.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(Roles.Customer, token.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal("test-issuer", token.Issuer);
    }

    [Fact]
    public void CreateToken_ExpiresInTheFuture()
    {
        var user = new ApplicationUser { Id = "user-1", Email = "kund@test.se" };

        var token = new JwtSecurityTokenHandler().ReadJwtToken(CreateService().CreateToken(user, Roles.Staff));

        Assert.True(token.ValidTo > DateTime.UtcNow);
        Assert.True(token.ValidTo <= DateTime.UtcNow.AddHours(2).AddMinutes(1));
    }
}
