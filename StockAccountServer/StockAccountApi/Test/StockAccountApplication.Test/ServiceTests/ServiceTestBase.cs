using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Repositories;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;
using StockAccountDomain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace StockAccountApplication.Test.ServiceTests;

public abstract class ServiceTestBase
{
    // FAKE SERVICE MOCKS
    protected Mock<IValidationService> ValidationServiceMock { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<UserManager<User>> UserManagerMock { get; }
    protected Mock<RoleManager<Role>> RoleManagerMock { get; }
    protected Mock<IMapper> MapperMock { get; }
    protected Mock<ITokenService> TokenServiceMock { get; }
    protected Mock<IConfiguration> ConfigurationMock { get; }
    protected Mock<IUserRepository> UserRepositoryMock { get; }
    protected Mock<IHttpContextAccessor> HttpContextAccessorMock { get; }
    protected Mock<IStockTransDomainService> StockTransDomainServiceMock { get; }

    // FAKE SEED DATA
    protected User TestUser { get; private set; }
    protected Company TestCompany { get; private set; }
    protected Stock TestStock { get; private set; }
    protected StockTrans TestStockTrans { get; private set; }
    protected JwtSecurityToken TestJwtToken { get; private set; }
    protected string TestRefreshToken { get; private set; }

    public ServiceTestBase()
    {
        ValidationServiceMock = new Mock<IValidationService>();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        MapperMock = new Mock<IMapper>();
        TokenServiceMock = new Mock<ITokenService>();
        ConfigurationMock = new Mock<IConfiguration>();
        UserRepositoryMock = new Mock<IUserRepository>();
        HttpContextAccessorMock = new Mock<IHttpContextAccessor>();
        StockTransDomainServiceMock = new Mock<IStockTransDomainService>();

        UserManagerMock = MockUserManager();
        RoleManagerMock = MockRoleManager();


        TestUser = TestDataFactory.CreateTestUser();
        TestCompany = TestDataFactory.CreateTestCompany();
        TestStock = TestDataFactory.CreateTestStock();
        TestStockTrans = TestDataFactory.CreateTestStockTrans();
        TestJwtToken = TestDataFactory.CreateTestJwtToken();
        TestRefreshToken = TestDataFactory.CreateTestRefreshToken();

    }

    private Mock<UserManager<User>> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private Mock<RoleManager<Role>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<Role>>();
        return new Mock<RoleManager<Role>>(store.Object, null, null, null, null);
    }

    protected void SetupTokenServiceReturnsSuccess()
    {
        TokenServiceMock
            .Setup(t => t.CreateToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .ReturnsAsync(TestJwtToken);

        TokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns(TestRefreshToken);

        ConfigurationMock
            .Setup(c => c["JWT:RefreshTokenValidityInDays"])
            .Returns("1"); 
    }

    protected void SetupAuthenticatedUser(string role = "user", string userName ="TestUser")
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Email, "test@mail.com"),
            new Claim(ClaimTypes.Name, userName ?? "")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = claimsPrincipal;

        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    }
}
