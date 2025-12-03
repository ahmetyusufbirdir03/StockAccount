using AutoMapper;
using Microsoft.AspNetCore.Http;
using StockAccountApplication.Services.UtilServices;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.User;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;

namespace StockAccountApplication.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public Task<ResponseDto<NoContentDto>> DeleteUserAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto<IList<UserResponseDto>>> GetAllUsers()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null)
            {
                return ResponseDto<IList<UserResponseDto>>
                    .Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
            }

            var isAdmin = currentUser.IsInRole("admin");

            if (!isAdmin)
            {
                return ResponseDto<IList<UserResponseDto>>
                    .Fail(ErrorMessageService.Unauthorized401, StatusCodes.Status401Unauthorized);
            }

            var users = await _unitOfWork.GetGenericRepository<User>().GetAllAsync();
            if (!users.Any())
                return ResponseDto<IList<UserResponseDto>>.Fail(ErrorMessageService.UserNotFound404, StatusCodes.Status404NotFound);

            IList<UserResponseDto> _users = _mapper.Map<IList<UserResponseDto>>(users);

            return ResponseDto<IList<UserResponseDto>>.Success(_users, StatusCodes.Status200OK);

        }

        public Task<ResponseDto<UserResponseDto>> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
