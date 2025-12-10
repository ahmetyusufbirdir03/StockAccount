using StockAccountDomain.Entities;
using StockAccountDomain.Enums;
using System.IdentityModel.Tokens.Jwt;

namespace StockAccountApplication.Test;

public static class TestDataFactory
{
    public static User CreateTestUser(
        Guid? id = null, 
        string name = "Test", string surname="User" , 
        string email = "test@mail.com", 
        string phoneNumber = "00000000001")
    {
        return new User
        {
            Id = id ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = name,
            Surname = surname,
            Email = email,
            PhoneNumber = phoneNumber,
        };
    }

    public static User CreateAdminUser()
    {
        return CreateTestUser(
            name: "System",
            surname: "Admin",
            email: "admin@system.com",
            phoneNumber: "99999999999",
            id: Guid.Parse("22222222-2222-2222-2222-222222222222")
        );
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

    public static Company CreateTestCompany(
        Guid? id = null,
        string companyName = "Test", 
        string address = "Comapny",
        string email = "test@mail.com",
        string phoneNumber = "00000000001")
    {
        return new Company
        {
            Id = id ?? Guid.NewGuid(),
            CompanyName = companyName,
            Address = address,
            Email = email,
            PhoneNumber = phoneNumber,
            DeletedAt = DateTime.Now.AddHours(1)
        };
    }

    public static Stock CreateTestStock(
        Guid? id = null,
        string name = "TestStock",
        int quantity = 5,
        UnitEnum unit = UnitEnum.Ton,
        decimal price = 10.2m,
        string description = "TestStock")
    {
        return new Stock
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Quantity = quantity,
            Unit = unit,
            Price = price,
            Description = description,
        };
    }

    public static StockTrans CreateTestStockTrans(
        int quantity = 5,
        StockTransTypeEnum type = StockTransTypeEnum.In,
        decimal unitPrice = 10.2m,
        string description = "TestStock")
    {
        return new StockTrans
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            StockId = Guid.NewGuid(),
            CounterpartyCompanyId = Guid.NewGuid(),
            Quantity = quantity,
            Type = type,
            UnitPrice = unitPrice,
            TotalPrice = quantity * unitPrice,
        };
    }

    public static Account CreateTestAccount(
        string name = "Account",
        string email = "email@mail.com",
        string phoneNumber = "00000000001",
        string address = "Address")
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            Email = name,
            AccountName = email,
            PhoneNumber = phoneNumber,
            Address = address
        };
    }

}