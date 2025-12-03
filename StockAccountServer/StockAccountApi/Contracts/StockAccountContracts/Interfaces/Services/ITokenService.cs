using StockAccountDomain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace StockAccountContracts.Interfaces.Services;

public interface ITokenService
{
    Task<JwtSecurityToken> CreateToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
}
