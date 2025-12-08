using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.User;
using StockAccountContracts.Dtos.User.Update;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;
using System.Security.Claims;

namespace StockAccountApplication.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IValidationService _validationService;

    public UserService(
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<User> userManager,
        IValidationService validationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _validationService = validationService;
    }
    public async Task<ResponseDto<NoContentDto>> DeleteUserAsync(Guid id)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser == null || !currentUser.IsInRole("admin"))
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
        }

        var users = await _unitOfWork
            .GetGenericRepository<User>()
            .GetAllAsync(x=> x.Id == id);
        var user = users.FirstOrDefault();
        if (user == null)
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.UserNotFound404, StatusCodes.Status404NotFound);
        }
        await _unitOfWork.GetGenericRepository<User>().DeleteAsync(user);

        return ResponseDto<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }

    public async Task<ResponseDto<IList<UserResponseDto>>> GetAllUsers()
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser == null || !currentUser.IsInRole("admin"))
        {
            return ResponseDto<IList<UserResponseDto>>
                .Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
        }

        var users = await _unitOfWork.GetGenericRepository<User>().GetAllAsync(x => x.DeletedAt == null);
        if (!users.Any())
            return ResponseDto<IList<UserResponseDto>>.Fail(ErrorMessageService.UserNotFound404, StatusCodes.Status404NotFound);

        IList<UserResponseDto> _users = _mapper.Map<IList<UserResponseDto>>(users);

        return ResponseDto<IList<UserResponseDto>>.Success(_users, StatusCodes.Status200OK);

    }

    public Task<ResponseDto<UserResponseDto>> GetUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<ResponseDto<NoContentDto>> SoftDeleteUserAsync(Guid userId)
    {
        var user = await _unitOfWork.GetGenericRepository<User>().GetByIdAsync(userId);

        if(user == null || user.DeletedAt != null)
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.UserNotFound404, StatusCodes.Status404NotFound);  
        }

        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "System";

        await _userManager.UpdateAsync(user);

        return ResponseDto<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }

    public async Task<ResponseDto<UserResponseDto>> UpdateUserAsync(UpdateUserRequestDto request)
    {
        //  Validator kontrolü
        var validationError = await _validationService.ValidateAsync<UpdateUserRequestDto, UserResponseDto>(request);
        if (validationError != null)
            return validationError;

        //  User bul
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null || user.DeletedAt != null)
            return ResponseDto<UserResponseDto>.Fail(ErrorMessageService.UserNotFound404, StatusCodes.Status404NotFound);

        // İstek atan kullanıcı =? değiştirilen kullanıcı
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == null || currentUserId != user.Id.ToString())
        {
            return ResponseDto<UserResponseDto>
                .Fail(ErrorMessageService.RestrictedAccess403, StatusCodes.Status401Unauthorized);
        }

        //  Tüm değerler boş mu kontrol et
        bool hasAnyValue =
            !string.IsNullOrWhiteSpace(request.Name) ||
            !string.IsNullOrWhiteSpace(request.Surname) ||
            !string.IsNullOrWhiteSpace(request.Email) ||
            !string.IsNullOrWhiteSpace(request.PhoneNumber);

        if (!hasAnyValue)
            return ResponseDto<UserResponseDto>.Fail(
                "Güncellenecek bir değer bulunamadı.",
                StatusCodes.Status400BadRequest
            );

        //  Username kontrol
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            if (request.Name == user.Name)
                return ResponseDto<UserResponseDto>.Fail(
                    "Yeni kullanıcı adı mevcut kullanıcı adıyla aynı olamaz.",
                    StatusCodes.Status400BadRequest
                );
            user.UserName = request.Name;
        }

        //  Email kontrol
        bool isEmailExist = await _unitOfWork.GetGenericRepository<User>().AnyAsync(x => x.Email == request.Email && x.Id != request.Id);
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (request.Email == user.Email)
                return ResponseDto<UserResponseDto>.Fail(
                    "Yeni email mevcut email ile aynı olamaz.",
                    StatusCodes.Status400BadRequest
                );
            if (isEmailExist)
            {
                return ResponseDto<UserResponseDto>.Fail(
                    "Bu mail adresi zaten kayıtlı.",
                    StatusCodes.Status400BadRequest
                );
            }
            user.Email = request.Email;
        }

        //  PhoneNumber kontrol
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            bool isPhoneNuberExist = await _unitOfWork.GetGenericRepository<User>().AnyAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != request.Id);
            if (request.PhoneNumber == user.PhoneNumber)
                return ResponseDto<UserResponseDto>.Fail(
                    "Yeni telefon numarası mevcut numarayla aynı olamaz.",
                    StatusCodes.Status400BadRequest
                );
            if (isPhoneNuberExist)
            {
                return ResponseDto<UserResponseDto>.Fail(
                    "Bu telefon numarası zaten kayıtlı.",
                    StatusCodes.Status400BadRequest
                );
            }
            user.PhoneNumber = request.PhoneNumber;
        }

        //  DB update
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ResponseDto<UserResponseDto>.Fail(updateResult.Errors.Select(e => e.Description).ToList(), StatusCodes.Status500InternalServerError);

        //  Response
        var responseDto = new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,

        };

        return ResponseDto<UserResponseDto>.Success(responseDto, StatusCodes.Status200OK);
    }
}
