using StockAccountDomain.Entities;
using StockAccountDomain.Models;
using StockAccountDomain.Respositories;
using StockAccountDomain.Services;

namespace StockAccountApplication.Services.DomainServices;

public class AccountCompanyDomainService : IAccountCompanyDomainService
{
    private readonly IAccountCompanyRepository _accountCompanyRepository;

    public AccountCompanyDomainService(IAccountCompanyRepository accountCompanyRepository)
    {
        _accountCompanyRepository = accountCompanyRepository;
    }

    public async Task<AccountCompany> CreateAccountCompanyAsync(AccountCompanyModel accountCompanyModel)
    {
        var accountCompany = new AccountCompany(accountCompanyModel.CompanyId, accountCompanyModel.AccountId);

        var response = await _accountCompanyRepository.CreateAccountCompanyAsync(accountCompany);
        
        return response;
    }
}
