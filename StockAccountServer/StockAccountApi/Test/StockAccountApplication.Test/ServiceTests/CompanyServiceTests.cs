using FluentAssertions;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Company;
using StockAccountContracts.Dtos.Company.Create;
using StockAccountDomain.Entities;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StockAccountContracts.Dtos.Company.Update;

namespace StockAccountApplication.Test.ServiceTests;

public class CompanyServiceTests : ServiceTestBase
{
    private readonly CompanyService _companyService;
    private readonly CreateCompanyRequestDto createRequestTest = new CreateCompanyRequestDto
    {
        UserId = Guid.NewGuid(),
        CompanyName = "Test Company",
        Email = "invalid email",
        PhoneNumber = "1234567890",
        Address = "123 Test St",
    };

    private readonly CompanyResponseDto TestCompanyResponseDto = new CompanyResponseDto
    {
        Id = Guid.NewGuid(),
        CompanyName = "Test Company",
        Email = "invalid email",
        PhoneNumber = "1234567890",
        Address = "123 Test St",
    };

    private readonly UpdateCompanyRequestDto TestUpdateCompanyRequestDto = new UpdateCompanyRequestDto
    {
        Id = Guid.NewGuid(),
        CompanyName = "Test Company",
        Email = "invalid email",
        PhoneNumber = "1234567890",
        Address = "123 Test St",
    };

    public CompanyServiceTests()
    {
        _companyService = new CompanyService(
            UnitOfWorkMock.Object,
            MapperMock.Object,
            ValidationServiceMock.Object,
            HttpContextAccessorMock.Object
        );
    }

    //Create Company Tests

    [Fact]
    public async Task CreateCompany_ShouldReturnValidationError_WhenRequestIsInvalid()
    {
        // Arrange
        var expectedError = ResponseDto<CompanyResponseDto>.Fail("ValidationError", 400);

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateCompanyRequestDto, CompanyResponseDto>(createRequestTest))
            .ReturnsAsync(expectedError);

        // Act
        var result = await _companyService.CreateCompany(createRequestTest);
        // Assert
        result.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task CreateCompany_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateCompanyRequestDto, CompanyResponseDto>(createRequestTest))
            .ReturnsAsync((ResponseDto<CompanyResponseDto>?)null);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(createRequestTest.UserId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _companyService.CreateCompany(createRequestTest);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.UserNotFound404, 404));
    }

    [Fact]
    public async Task CreateCompany_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateCompanyRequestDto, CompanyResponseDto>(createRequestTest))
            .ReturnsAsync((ResponseDto<CompanyResponseDto>)null);

        UnitOfWorkMock
            .Setup(u => u
            .GetGenericRepository<User>()
            .GetByIdAsync(createRequestTest.UserId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(TestUser);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetAllAsync(
                It.IsAny<Expression<Func<Company, bool>>>(), null))
            .ReturnsAsync(new List<Company> { TestCompany });

        //Act
        var result = await _companyService.CreateCompany(createRequestTest);

        //Assert
        result.Should().BeEquivalentTo(ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.EmailAlreadyRegistered409, 409));
    }

    //[Fact]
    //public async Task CreateCompany_ShouldReturnConflict_WhenPhoneNumberAlreadyExists()
    //{
    //    // Arrange
    //    ValidationServiceMock
    //        .Setup(v => v.ValidateAsync<CreateCompanyRequestDto, CompanyResponseDto>(createRequestTest))
    //        .ReturnsAsync((ResponseDto<CompanyResponseDto>)null!);

    //    UnitOfWorkMock
    //        .Setup(u => u.GetGenericRepository<User>().GetByIdAsync(createRequestTest.UserId))
    //        .ReturnsAsync(TestUser);

    //    UnitOfWorkMock
    //        .Setup(u => u.GetGenericRepository<Company>()
    //        .GetAllAsync(It.IsAny<Expression<Func<Company,bool>>>(),null))
    //        .ReturnsAsync(new List<Company> { });

    //    UnitOfWorkMock
    //        .Setup(u => u.GetGenericRepository<Company>()
    //        .GetAsync(It.IsAny<Expression<Func<Company, bool>>>()))
    //        .ReturnsAsync(TestCompany);

    //    //Act
    //    var result = await _companyService.CreateCompany(createRequestTest);
    //    //Assert
    //    result.Should().BeEquivalentTo(ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.PhoneNumberAlreadyRegistered409, 409));
    //}

    [Fact]
    public async Task CreateCompany_ShouldReturnBadRequest_WhenUserHasThreeCompanies()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateCompanyRequestDto, CompanyResponseDto>(createRequestTest))
            .ReturnsAsync((ResponseDto<CompanyResponseDto>?)null);

        TestUser.Companies = new List<Company>
        {
            new Company(), new Company(), new Company()
        };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(createRequestTest.UserId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(TestUser);

        // Act
        var result = await _companyService.CreateCompany(createRequestTest);

        // Assert
        result.Should().BeEquivalentTo(
            ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.MaxCompanyLimitReached400, 400)
        );
    }

    [Fact]
    public async Task CreateCompany_ShouldCreateCompany_WhenRequestIsValid()
    {
        // Arrange
        ValidationServiceMock
            .Setup(v => v.ValidateAsync<CreateCompanyRequestDto, CompanyResponseDto>(createRequestTest))
            .ReturnsAsync((ResponseDto<CompanyResponseDto>?)null);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(createRequestTest.UserId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(TestUser);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetAllAsync(
                It.IsAny<Expression<Func<Company, bool>>>(), null))
            .ReturnsAsync(new List<Company> { });

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().CreateAsync(It.IsAny<Company>()))
            .ReturnsAsync(TestCompany);

        MapperMock
            .Setup(m => m.Map<Company>(createRequestTest))
            .Returns(TestCompany);

        MapperMock
            .Setup(m => m.Map<CompanyResponseDto>(TestCompany))
            .Returns(TestCompanyResponseDto);
        // Act
        var result = await _companyService.CreateCompany(createRequestTest);
        // Assert
        result.Should().BeEquivalentTo(ResponseDto<CompanyResponseDto>.Success(TestCompanyResponseDto, 201));
    }


    //Soft Delete Company Tests

    [Fact]
    public async Task SoftDeleteCompany_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetAsync(It.IsAny<Expression<Func<Company, bool>>>()))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _companyService.SoftDeleteCompany(companyId);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Fail(ErrorMessageService.CompanyNotFound404));
    }

    [Fact]
    public async Task SoftDeleteCompany_ShouldSoftDeleteCompany_WhenCompanyExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
                .GetAsync(It.IsAny<Expression<Func<Company, bool>>>()))
            .ReturnsAsync(TestCompany);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().UpdateAsync(TestCompany))
            .ReturnsAsync(TestCompany);

        SetupAuthenticatedUser(role: "user", userName: "TestUser");

        // Act
        var result = await _companyService.SoftDeleteCompany(companyId);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<NoContentDto>.Success(204));
        TestCompany.DeletedAt.Should().NotBeNull();
        TestCompany.DeletedBy.Should().Be("test@mail.com");
    }


    //Get All Companies Tests  
    [Fact]
    public async Task GetAllCompanies_ShouldReturnUnauthorized_WhenUserIsNullOrNotAdmin()
    {
        // Arrange
        SetupAuthenticatedUser(role: "user");

        // Act
        var result = await _companyService.GetAllCompanies();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<CompanyResponseDto>>.Fail(ErrorMessageService.Unauthorized401, 401));
    }

    [Fact]
    public async Task GetAllCompanies_ShouldReturnNotFound_WhenNoCompanyExists()
    {
        // Arrange
        SetupAuthenticatedUser(role: "admin");
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
            .GetAllAsync(c => c.DeletedAt == null, null))
            .ReturnsAsync(new List<Company>());

        // Act
        var result = await _companyService.GetAllCompanies();

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<CompanyResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task GetAllCompanies_ShouldReturnAllCompanies_WhenCompaniesExist()
    {
        // Arrange
        SetupAuthenticatedUser(role: "admin");
        var testCompanyList = new List<Company>
        {
            TestCompany,
            TestDataFactory.CreateTestCompany()
        };

        var testCompanyResponseDtoList = new List<CompanyResponseDto>
        {
            new CompanyResponseDto(),
            TestCompanyResponseDto
        };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
            .GetAllAsync(c => c.DeletedAt == null, null))
            .ReturnsAsync(testCompanyList);

        MapperMock
            .Setup(m => m.Map<IList<CompanyResponseDto>>(testCompanyList))
            .Returns(testCompanyResponseDtoList);


        // Act
        var result = await _companyService.GetAllCompanies();

        // Assert
        result.Should().BeEquivalentTo(
            ResponseDto<IList<CompanyResponseDto>>.Success(
                testCompanyResponseDtoList,
                200
            )
        );

    }


    // Get User Companies Tests

    [Fact]
    public async Task GetUserCompanies_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _companyService.GetUserCompanies(userId);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<CompanyResponseDto>>.Fail(ErrorMessageService.UserNotFound404, 404));
    }

    [Fact]
    public async Task GetUserCompanies_ShouldReturnRestricted_WhenCurrentUserIdDoesNotMatch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataFactory.CreateTestUser(id: userId, name: "Owner", email: "owner@test.com");
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(user);

        var differentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "TestAuth"));
        HttpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = differentPrincipal });

        // Act
        var result = await _companyService.GetUserCompanies(userId);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<CompanyResponseDto>>.Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status403Forbidden));
    }

    [Fact]
    public async Task GetUserCompanies_ShouldReturnNotFound_WhenUserHasNoCompanies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataFactory.CreateTestUser(id: userId, name: "Owner", email: "owner@test.com");
        user.Companies = new List<Company>();

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
                .GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(user);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "TestAuth"));
        HttpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = principal });

        // Act
        var result = await _companyService.GetUserCompanies(userId);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<CompanyResponseDto>>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task GetUserCompanies_ShouldReturnUserCompanies_WhenAuthorizedAndCompaniesExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataFactory.CreateTestUser(id: userId, name: "Owner", email: "owner@test.com");
        user.Companies = new List<Company>
        {
            TestCompany,
            TestDataFactory.CreateTestCompany()
        };

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(user);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "TestAuth"));
        HttpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = principal });

        var companyDtoList = new List<CompanyResponseDto>
        {
            TestCompanyResponseDto,
            new CompanyResponseDto()
        };

        MapperMock
            .Setup(m => m.Map<IList<CompanyResponseDto>>(It.IsAny<ICollection<Company>>()))
            .Returns(companyDtoList);

        // Act
        var result = await _companyService.GetUserCompanies(userId);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<IList<CompanyResponseDto>>.Success(companyDtoList, 200));
    }


    // Update Company Tests

    [Fact]
    public async Task UpdateCompany_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().GetByIdAsync(TestCompany.Id,null))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _companyService.UpdateCompany(TestUpdateCompanyRequestDto);

        // Assert
        result.Should().BeEquivalentTo(ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.CompanyNotFound404, 404));
    }

    [Fact]
    public async Task UpdateCompany_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
            .GetByIdAsync(TestUpdateCompanyRequestDto.Id, It.IsAny<Expression<Func<Company, object>>[]>()))
            .ReturnsAsync(TestCompany);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(TestCompany.UserId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _companyService.UpdateCompany(TestUpdateCompanyRequestDto);

        // Assert
        result.Should().BeEquivalentTo(
            ResponseDto<CompanyResponseDto>.Fail(ErrorMessageService.UserNotFound404, 404)
        );
    }

    [Fact]
    public async Task UpdateCompany_ShouldReturnUnauthorized_WhenCurrentUserIsNotOwner()
    {
        // Arrange 
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
            .GetByIdAsync(TestUpdateCompanyRequestDto.Id, It.IsAny<Expression<Func<Company, object>>[]>()))
            .ReturnsAsync(TestCompany);

        var differentUser = TestDataFactory.CreateTestUser(id: Guid.NewGuid());
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(TestCompany.UserId, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(differentUser);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
    }, "TestAuth"));
        HttpContextAccessorMock.Setup(h => h.HttpContext)
            .Returns(new DefaultHttpContext { User = principal });

        // Act
        var result = await _companyService.UpdateCompany(TestUpdateCompanyRequestDto);

        // Assert
        result.Should().BeEquivalentTo(
            ResponseDto<CompanyResponseDto>.Fail(
                ErrorMessageService.RestrictedAccess403,
                StatusCodes.Status403Forbidden
            )
        );
    }

    [Fact]
    public async Task UpdateCompany_ShouldUpdateCompany_WhenRequestIsValid()
    {
        // Arrange 
        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>()
            .GetByIdAsync(TestUpdateCompanyRequestDto.Id, It.IsAny<Expression<Func<Company, object>>[]>()))
            .ReturnsAsync(TestCompany);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>()
            .GetByIdAsync(
                    TestCompany.UserId, 
                    It.IsAny<Expression<Func<User, object>>[]>()
                )
            )
            .ReturnsAsync(TestUser);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, TestUser.Id.ToString())
    }, "TestAuth"));
        HttpContextAccessorMock.Setup(h => h.HttpContext)
            .Returns(new DefaultHttpContext { User = principal });

        MapperMock
            .Setup(m => m.Map(TestUpdateCompanyRequestDto, TestCompany))
            .Returns(TestCompany);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<Company>().UpdateAsync(TestCompany))
            .ReturnsAsync(TestCompany);

        MapperMock
            .Setup(m => m.Map<CompanyResponseDto>(TestCompany))
            .Returns(TestCompanyResponseDto);

        // Act
        var result = await _companyService.UpdateCompany(TestUpdateCompanyRequestDto);

        // Assert
        result.Should().BeEquivalentTo(
            ResponseDto<CompanyResponseDto>.Success(TestCompanyResponseDto, 200)
        );
    }



}
