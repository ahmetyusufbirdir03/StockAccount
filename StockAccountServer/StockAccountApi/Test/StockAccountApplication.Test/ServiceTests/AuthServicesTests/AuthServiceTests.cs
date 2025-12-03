using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using StockAccountApplication.Services;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Auth;
using StockAccountContracts.Dtos.Auth.Login;
using StockAccountContracts.Dtos.Auth.Register;
using StockAccountDomain.Entities;

namespace StockAccountApplication.Test.ServiceTests.AuthServicesTests;

public class AuthServiceTests : ServiceTestBase
{
    private readonly AuthService _authService;

    private readonly RegisterRequestDto _registerRequest = new()
    {
        Name = "Deneme",
        Surname = "Dummy",
        Email = "test@mail.com",
        Password = "ahmet",
        PhoneNumber = "1234567890"
    };

    private readonly LoginRequestDto _loginRequest = new()
    {
        Email = "test@mail.com",
        Password = "ahmet"
    };

    public AuthServiceTests()
    {
        _authService = new AuthService(
            MapperMock.Object,
            UserRepositoryMock.Object,
            UserManagerMock.Object,
            TokenServiceMock.Object,
            ConfigurationMock.Object,
            ValidationServiceMock.Object,
            RoleManagerMock.Object,
            UnitOfWorkMock.Object,
            HttpContextAccessorMock.Object
        );
    }

    // REGISTER TESTS
    [Fact]
    public async Task RegisterAsync_Should_Return_ValidationError_When_Validation_Fails()
    {
        // Arrange
        var expectedError = ResponseDto<AuthResponseDto>.Fail("ValidationError", 400);

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<RegisterRequestDto, AuthResponseDto>(_registerRequest))
            .ReturnsAsync(expectedError);

        // Act
        var result = await _authService.RegisterAsync(_registerRequest);

        // Assert
        result.Should().Be(expectedError);
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_EmailConflict_When_Email_Exists()
    {
        // Arrange
        var expectedError = ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.EmailAlreadyRegistered409, 409);

        ValidationServiceMock
           .Setup(v => v.ValidateAsync<RegisterRequestDto, AuthResponseDto>(It.IsAny<RegisterRequestDto>()))
           .ReturnsAsync((ResponseDto<AuthResponseDto>)null);

        UserManagerMock
           .Setup(x => x.FindByEmailAsync(_registerRequest.Email!))
           .ReturnsAsync(new User { Email = _registerRequest.Email });

        // Act
        var result = await _authService.RegisterAsync(_registerRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_PhoneNumberConflict_When_PhoneNumber_Exists()
    {
        // Arrange
        var expectedError = ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.PhoneNumberAlreadyRegistered409, 409);

        ValidationServiceMock
           .Setup(v => v.ValidateAsync<RegisterRequestDto, AuthResponseDto>(It.IsAny<RegisterRequestDto>()))
           .ReturnsAsync((ResponseDto<AuthResponseDto>)null!);

        UserManagerMock
           .Setup(x => x.FindByEmailAsync(_registerRequest.Email!))
           .ReturnsAsync((User)null);

        UserRepositoryMock
           .Setup(r => r.GetUserByPhoneNumberAsync(_registerRequest.PhoneNumber!))
           .ReturnsAsync(new User { PhoneNumber = _registerRequest.PhoneNumber });

        // Act
        var result = await _authService.RegisterAsync(_registerRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_Success_When_Data_Is_Valid()
    {
        // Arrange
        SetupTokenServiceReturnsSuccess();

        var expectedResponse = ResponseDto<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = "token_string_kontrolu_yapmiyoruz_mocktan_geliyor",
            RefreshToken = TestRefreshToken,
            Expiration = DateTime.Now.AddHours(1)
        }, 201);

        ValidationServiceMock
           .Setup(v => v.ValidateAsync<RegisterRequestDto, AuthResponseDto>(It.IsAny<RegisterRequestDto>()))
           .ReturnsAsync((ResponseDto<AuthResponseDto>)null!);

        UserManagerMock.Setup(x => x.FindByEmailAsync(_registerRequest.Email!)).ReturnsAsync((User)null);
        UserRepositoryMock.Setup(r => r.GetUserByPhoneNumberAsync(_registerRequest.PhoneNumber!))!.ReturnsAsync((User)null!);

        MapperMock.Setup(m => m.Map<User>(_registerRequest)).Returns(TestUser);

        // User Creation
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), _registerRequest.Password!)).ReturnsAsync(IdentityResult.Success);

        // Role Control
        RoleManagerMock.Setup(r => r.RoleExistsAsync("user")).ReturnsAsync(true);
        UserManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), "user")).ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "user" });

        // Update
        UserManagerMock.Setup(u => u.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync(_registerRequest);

        // Assert
        result.Data.RefreshToken.Should().Be(TestRefreshToken);
        result.StatusCode.Should().Be(201);

        UserManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), _registerRequest.Password!), Times.Once);
        UserManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), "user"), Times.Once);

        TokenServiceMock.Verify(t => t.CreateToken(It.IsAny<User>(), It.IsAny<IList<string>>()), Times.Once);
    }


    // LOGIN TESTS 

    [Fact]
    public async Task LoginAsync_Should_Return_ValidationError_When_Validation_Fails()
    {
        // Arrange
        var expectedError = ResponseDto<AuthResponseDto>.Fail("ValidationError", 400);

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<LoginRequestDto, AuthResponseDto>(_loginRequest))
            .ReturnsAsync(expectedError);

        // Act
        var result = await _authService.LoginAsync(_loginRequest);

        // Assert
        result.Should().Be(expectedError);
    }

    [Fact]
    public async Task LoginAsync_Should_Return_UserNotFound_When_Email_Not_Exist()
    {
        // Arrange
        var expectedError = ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.UserNotFound404, 404);

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<LoginRequestDto, AuthResponseDto>(_loginRequest))
            .ReturnsAsync((ResponseDto<AuthResponseDto>)null);

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(_loginRequest.Email))
            .ReturnsAsync((User)null);

        // Act
        var result = await _authService.LoginAsync(_loginRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Invalid_Authentication_Cridentials_When_Password_Is_Wrong()
    {
        // Arrange
        var foundUser = new User { Email = _loginRequest.Email };
        var expectedError = ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.InvalidAuthenticationCredentials401, 401);

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<LoginRequestDto, AuthResponseDto>(_loginRequest))
            .ReturnsAsync((ResponseDto<AuthResponseDto>)null);

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(_loginRequest.Email))
            .ReturnsAsync(foundUser);

        UserManagerMock
            .Setup(c => c.CheckPasswordAsync(foundUser, _loginRequest.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.LoginAsync(_loginRequest);

        // Assert
        result.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Success_When_Data_Is_Valid()
    {
        // Arrange
        SetupTokenServiceReturnsSuccess();

        var foundUser = TestUser;

        var expectedResponse = ResponseDto<AuthResponseDto>.Success(new AuthResponseDto
        {
            RefreshToken = TestRefreshToken 
        }, 200);

        ValidationServiceMock
            .Setup(v => v.ValidateAsync<LoginRequestDto, AuthResponseDto>(_loginRequest))
            .ReturnsAsync((ResponseDto<AuthResponseDto>)null);

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(_loginRequest.Email))
            .ReturnsAsync(foundUser);

        UserManagerMock
            .Setup(c => c.CheckPasswordAsync(foundUser, _loginRequest.Password))
            .ReturnsAsync(true);

        UserManagerMock.Setup(u => u.GetRolesAsync(foundUser)).ReturnsAsync(new List<string> { "user" });

        UserManagerMock.Setup(u => u.UpdateAsync(foundUser)).ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(u => u.UpdateSecurityStampAsync(foundUser)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.LoginAsync(_loginRequest);

        // Assert
        result.Data.RefreshToken.Should().Be(TestRefreshToken);
        result.StatusCode.Should().Be(200);

        UserManagerMock.Verify(c => c.CheckPasswordAsync(foundUser, _loginRequest.Password), Times.Once);
        TokenServiceMock.Verify(t => t.CreateToken(foundUser, It.IsAny<IList<string>>()), Times.Once);
        UserManagerMock.Verify(u => u.UpdateAsync(foundUser), Times.Once);
        UserManagerMock.Verify(u => u.UpdateSecurityStampAsync(foundUser), Times.Once);
    }

    //REFRESH TOKEN TESTS
    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Token_Not_Found_When_Refresh_Token_Is_Not_Exist()
    {
        // Arrange
        string refreshToken = null;
        var expectedError = ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.TokenNotFound404, 404);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(refreshToken);
        // Assert
        result.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_InvalidOrExpiredToken_When_Refresh_Token_Is_Not_Valid()
    {
        // Arrange
        string refreshToken = TestRefreshToken;
        var expectedError = ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.InvalidOrExpiredToken401, 401);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>().GetAsync(x => x.RefreshToken == refreshToken))
            .ReturnsAsync((User)null);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(refreshToken);
        // Assert
        result.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_SessionExpired_When_Refresh_Token_Is_Expired()
    {
        // Arrange
        string refreshToken = TestRefreshToken;
        var expectedError = ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.SessionExpired401, 401);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>().GetAsync(x => x.RefreshToken == refreshToken))
            .ReturnsAsync(TestUser);

        TestUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-5);
        TestUser.RefreshToken = null;

        UserManagerMock.Setup(u => u.UpdateAsync(TestUser)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RefreshAccessTokenAsync(refreshToken);

        // Assert
        result.Should().BeEquivalentTo(expectedError);
    }

    [Fact]
    public async Task ResfreshTokenAsync_Should_Return_Success_When_Refresh_Token_Is_Valid()
    {
        // Arrange
        string refreshToken = TestRefreshToken;
        SetupTokenServiceReturnsSuccess();

        var expectedResponse = ResponseDto<AuthResponseDto>.Success(new AuthResponseDto
        {
            RefreshToken = TestRefreshToken
        }, 200);

        UnitOfWorkMock
            .Setup(u => u.GetGenericRepository<User>().GetAsync(x => x.RefreshToken == refreshToken))
            .ReturnsAsync(TestUser);

        TestUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(10); 
        TestUser.RefreshToken = refreshToken;

        UserManagerMock.Setup(u => u.GetRolesAsync(TestUser)).ReturnsAsync(new List<string> { "user" });

        UserManagerMock.Setup(u => u.UpdateAsync(TestUser)).ReturnsAsync(IdentityResult.Success);
        // Act
        var result = await _authService.RefreshAccessTokenAsync(refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().BeEquivalentTo(expectedResponse, options => options
            .Excluding(x => x.Data!.AccessToken)
            .Excluding(x => x.Data!.Expiration));
        result.StatusCode.Should().Be(200);

        TokenServiceMock.Verify(t => t.CreateToken(It.IsAny<User>(), It.IsAny<IList<string>>()), Times.Once);
        UserManagerMock.Verify(u => u.UpdateAsync(TestUser), Times.Once);
    }
}