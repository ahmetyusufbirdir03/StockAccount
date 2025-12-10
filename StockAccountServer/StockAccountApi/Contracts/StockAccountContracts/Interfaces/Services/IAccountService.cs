using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Account;
using StockAccountContracts.Dtos.Account.Create;

namespace StockAccountContracts.Interfaces.Services;

public interface IAccountService
{
    Task<ResponseDto<AccountResponseDto>> CreateAccountAsync(CreateAccountRequestDto Request);
}
