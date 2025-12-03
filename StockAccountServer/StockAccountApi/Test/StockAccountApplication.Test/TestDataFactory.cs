using StockAccountContracts.Dtos.Auth.Register;
using StockAccountDomain.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace StockAccountApplication.Test;

public static class TestDataFactory
{
    public static User CreateTestUser(string name = "Test", string surname="User" , string email = "test@mail.com", string phoneNumber = "00000000001")
    {
        return new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = name,
            Surname = surname,
            Email = email,
            PhoneNumber = phoneNumber,
        };
    }

    public static JwtSecurityToken CreateTestJwtToken()
    {
        return new JwtSecurityToken(
            issuer: "test_issuer",
            audience: "test_audience",
            expires: DateTime.Now.AddHours(1)
        );
    }

    public static string CreateTestRefreshToken()
    {
        return "mock_refresh_token_xyz_123";
    }
}