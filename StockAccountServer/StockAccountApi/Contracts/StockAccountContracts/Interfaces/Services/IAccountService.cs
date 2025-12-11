using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Account;
using StockAccountContracts.Dtos.Account.Create;
using StockAccountContracts.Dtos.Account.Update;

namespace StockAccountContracts.Interfaces.Services;

public interface IAccountService
{
    /// <summary>
    ///  Creates account by given values.
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    Task<ResponseDto<AccountResponseDto>> CreateAccountAsync(CreateAccountRequestDto Request);

    /// <summary>
    ///     Updates account by given request
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    Task<ResponseDto<AccountResponseDto>> UpdateAccountAsync(UpdateAccountRequestDto Request);

    /// <summary>
    ///  Gets all accounts, admin function.
    /// </summary>
    /// <returns></returns>
    Task<ResponseDto<IList<AccountResponseDto>>> GetAllAccountsAsync();

    /// <summary>
    ///  Gets account list by company id.
    /// </summary>
    /// <param name="CompanyId"></param>
    /// <returns></returns>
    Task<ResponseDto<IList<AccountResponseDto>>> GetAccountsByCompanyIdAsync(Guid CompanyId);
    
    /// <summary>
    ///     Deletes the account of given id
    /// </summary>
    /// <param name="AccountId"></param>
    /// <returns></returns>
    Task <ResponseDto<NoContentDto>> DeleteAccountAsync(Guid AccountId);
}

