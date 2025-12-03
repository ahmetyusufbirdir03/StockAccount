using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Auth;
using StockAccountContracts.Dtos.Auth.Login;
using StockAccountContracts.Dtos.Auth.Register;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Repositories;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace StockAccountApplication.Services; 

public class AuthService : IAuthService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IValidationService _validationService;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IMapper mapper,
        IUserRepository userRepository,
        UserManager<User> userManager,
        ITokenService tokenService,
        IConfiguration configuration,
        IValidationService validationService,
        RoleManager<Role> roleManager,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _validationService = validationService;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ResponseDto<AuthResponseDto>> RefreshAccessTokenAsync(string refreshToken)
    {
        if(refreshToken == null)
        {
            return ResponseDto<AuthResponseDto>
                .Fail(ErrorMessageService.TokenNotFound404, StatusCodes.Status404NotFound);
        }  
        
        var user = await _unitOfWork.GetGenericRepository<User>().GetAsync(x => x.RefreshToken == refreshToken);
        if (user == null)
            return ResponseDto<AuthResponseDto>
                .Fail(ErrorMessageService.InvalidOrExpiredToken401, StatusCodes.Status401Unauthorized);

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
            return ResponseDto<AuthResponseDto>
                .Fail(ErrorMessageService.SessionExpired401, StatusCodes.Status401Unauthorized);
        }

        user.SecurityStamp = Guid.NewGuid().ToString();
        IList<string> roles = await _userManager.GetRolesAsync(user);
        JwtSecurityToken newAccessToken = await _tokenService.CreateToken(user, roles);

        await _userManager.UpdateAsync(user);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken);

        return ResponseDto<AuthResponseDto>.Success(
        new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = user.RefreshToken,
            Expiration = newAccessToken.ValidTo

        }, StatusCodes.Status200OK);

    }

    public async Task<ResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var _request = request;

        // Validation Rules Check
        var validationError = await _validationService.ValidateAsync<RegisterRequestDto, AuthResponseDto>(request);
        if (validationError != null)
            return validationError;

        // Email Conflict Check
        var isEmailExist = await _userManager.FindByEmailAsync(request.Email!);
        if (isEmailExist is not null)
            return ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.EmailAlreadyRegistered409, StatusCodes.Status409Conflict);

        // Phone Number Conflict Check
        var isPhoneNumberExist = await _userRepository.GetUserByPhoneNumberAsync(request.PhoneNumber!);
        if (isPhoneNumberExist is not null)
        {
            return ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.PhoneNumberAlreadyRegistered409, StatusCodes.Status409Conflict);
        }

        User user = _mapper.Map<User>(request);
        user.UserName = request.Email;
        user.SecurityStamp = Guid.NewGuid().ToString();

        IdentityResult result = await _userManager.CreateAsync(user, request.Password!);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync("user"))
                await _roleManager.CreateAsync(new Role
                {
                    Name = "user",
                    NormalizedName = "USER",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                });

            await _userManager.AddToRoleAsync(user, "user");
        }

        //Create Access Token
        IList<string> roles = await _userManager.GetRolesAsync(user);

        JwtSecurityToken token = await _tokenService.CreateToken(user, roles);
        string refreshToken = _tokenService.GenerateRefreshToken();

        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

        string _token = new JwtSecurityTokenHandler().WriteToken(token);

        await _userManager.UpdateAsync(user);

        return ResponseDto<AuthResponseDto>.Success(
            new AuthResponseDto
            {
                AccessToken = _token,
                RefreshToken = refreshToken,
                Expiration = token.ValidTo
            },
            StatusCodes.Status201Created);
    }

    public async Task<ResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var validationError = await _validationService.ValidateAsync<LoginRequestDto, AuthResponseDto>(request);
        if (validationError != null)
            return validationError;


        User? user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.UserNotFound404, StatusCodes.Status404NotFound);
        }

        bool checkPassword = await _userManager.CheckPasswordAsync(user, request.Password);

        if (user == null || !checkPassword)
            return ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.InvalidAuthenticationCredentials401, StatusCodes.Status401Unauthorized);

        IList<string> roles = await _userManager.GetRolesAsync(user);

        JwtSecurityToken token = await _tokenService.CreateToken(user, roles);

        user.RefreshToken = null;
        string refreshToken = _tokenService.GenerateRefreshToken();

        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
        user.SecurityStamp = Guid.NewGuid().ToString();

        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        string _token = new JwtSecurityTokenHandler().WriteToken(token);

        return ResponseDto<AuthResponseDto>
            .Success(
                new AuthResponseDto
                {
                    AccessToken = _token,
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                },
                StatusCodes.Status200OK
            );
    }

    public async Task<ResponseDto<AuthResponseDto>> RegisterAdminAsync(RegisterRequestDto request)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if(currentUser == null || !currentUser.IsInRole("admin"))
        {
            return ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
        }

        // Validation Rules Check
        var validationError = await _validationService.ValidateAsync<RegisterRequestDto, AuthResponseDto>(request);
        if (validationError != null)
            return validationError;

        // Email Conflict Check
        var isEmailExist = await _userManager.FindByEmailAsync(request.Email!);
        if (isEmailExist is not null)
            return ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.EmailAlreadyRegistered409, StatusCodes.Status409Conflict);

        // Phone Number Conflict Check
        var isPhoneNumberExist = await _userRepository.GetUserByPhoneNumberAsync(request.PhoneNumber!);
        if (isPhoneNumberExist is not null)
        {
            return ResponseDto<AuthResponseDto>.Fail(ErrorMessageService.PhoneNumberAlreadyRegistered409, StatusCodes.Status409Conflict);
        }

        User user = _mapper.Map<User>(request);
        user.UserName = request.Email;
        user.SecurityStamp = Guid.NewGuid().ToString();

        IdentityResult result = await _userManager.CreateAsync(user, request.Password!);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync("admin"))
                await _roleManager.CreateAsync(new Role
                {
                    Name = "admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                });

            await _userManager.AddToRoleAsync(user, "admin");
        }

        //Create Access Token
        IList<string> roles = await _userManager.GetRolesAsync(user);

        JwtSecurityToken token = await _tokenService.CreateToken(user, roles);
        string refreshToken = _tokenService.GenerateRefreshToken();

        _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

        string _token = new JwtSecurityTokenHandler().WriteToken(token);

        await _userManager.UpdateAsync(user);

        return ResponseDto<AuthResponseDto>.Success(
            new AuthResponseDto
            {
                AccessToken = _token,
                RefreshToken = refreshToken,
                Expiration = token.ValidTo
            },
            StatusCodes.Status201Created);
    }
}
