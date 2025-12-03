using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Auth;
using StockAccountContracts.Dtos.Auth.Login;
using StockAccountContracts.Dtos.Auth.Register;

namespace StockAccountContracts.Interfaces.Services;

public interface IAuthService
{
    public Task<ResponseDto<AuthResponseDto>> RefreshAccessTokenAsync(string refreshToken);
    public Task<ResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    public Task<ResponseDto<AuthResponseDto>> RegisterAdminAsync(RegisterRequestDto request);
    public Task<ResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request);
}
