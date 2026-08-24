using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ControleFacil.Api.Extensions;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using ControleFacil.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ControleFacil.Api.Tests.Auth;

// Testes puros de JWT/claims (sem banco) — cobrem a claim "role" adicionada na Sprint
// Admin-1: precisa estar no token gerado E o GetRole() precisa recusar (não assumir
// um valor por padrão) quando a claim está ausente ou inválida.
public class RoleClaimTests
{
    private static IConfiguration BuildConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "chave-de-teste-bem-longa-para-assinatura-hmac-0123456789",
            ["Jwt:Issuer"] = "ControleFacilTests",
            ["Jwt:Audience"] = "ControleFacilTestsUsers",
            ["Jwt:ExpireMinutes"] = "60",
        })
        .Build();

    private static ClaimsPrincipal PrincipalWithRole(string role)
    {
        var identity = new ClaimsIdentity(new[] { new Claim("role", role) });
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void GenerateToken_IncludesRoleClaim_MatchingUserRole()
    {
        var service = new JwtTokenService(BuildConfig());
        var user = new User { Id = 1, Name = "Admin", Email = "admin@teste.com", Role = UserRole.Admin };

        var token = service.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var roleClaim = Assert.Single(jwt.Claims, c => c.Type == "role");

        Assert.Equal("Admin", roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_RegularUser_RoleClaimIsUser()
    {
        var service = new JwtTokenService(BuildConfig());
        var user = new User { Id = 2, Name = "Comum", Email = "comum@teste.com", Role = UserRole.User };

        var token = service.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("User", Assert.Single(jwt.Claims, c => c.Type == "role").Value);
    }

    [Fact]
    public void GetRole_ClaimPresent_ParsesEnumCorrectly()
    {
        var principal = PrincipalWithRole("Admin");
        Assert.Equal(UserRole.Admin, principal.GetRole());
    }

    [Fact]
    public void GetRole_ClaimMissing_ThrowsInsteadOfSilentFallback()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        Assert.Throws<InvalidOperationException>(() => principal.GetRole());
    }

    [Fact]
    public void GetRole_ClaimInvalid_ThrowsInsteadOfSilentFallback()
    {
        var principal = PrincipalWithRole("NotARealRole");
        Assert.ThrowsAny<Exception>(() => principal.GetRole());
    }
}
