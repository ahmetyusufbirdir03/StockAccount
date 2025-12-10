using StockAccountDomain.Entities;
using StockAccountDomain.Models;

namespace StockAccountDomain.Services;

public interface IAccountCompanyDomainService
{
    Task<AccountCompany> CreateAccountCompanyAsync(AccountCompanyModel accountCompanyModel);
}
