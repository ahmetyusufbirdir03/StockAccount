using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.User;
using StockAccountContracts.Dtos.User.Update;

namespace StockAccountContracts.Interfaces.Services;

public interface IUserService
{
    public Task<ResponseDto<NoContentDto>> DeleteUserAsync(Guid id);
    public Task<ResponseDto<NoContentDto>> SoftDeleteUserAsync(Guid userId);
    public Task<ResponseDto<UserResponseDto>> UpdateUserAsync(UpdateUserRequestDto request);
    public Task<ResponseDto<IList<UserResponseDto>>> GetAllUsers();
    public Task<ResponseDto<UserResponseDto>> GetUserByEmailAsync(string email);
}

