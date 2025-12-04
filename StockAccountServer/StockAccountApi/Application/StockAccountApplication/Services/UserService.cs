using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.User;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;

namespace StockAccountApplication.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public UserService(
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<User> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }
    public async Task<ResponseDto<NoContentDto>> DeleteUserAsync(Guid id)
    {
        var users = await _unitOfWork
            .GetGenericRepository<User>()
            .GetAllAsync(x=> x.Id == id && x.DeletedAt == null);
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

        if(user == null)
        {
            return ResponseDto<NoContentDto>.Fail(ErrorMessageService.UserNotFound404, StatusCodes.Status404NotFound);  
        }

        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

        await _userManager.UpdateAsync(user);

        return ResponseDto<NoContentDto>.Success(StatusCodes.Status204NoContent);
    }
}
