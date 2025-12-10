using StockAccountDomain.Entities;

namespace StockAccountDomain.Respositories;

public interface IAccountCompanyRepository
{
    Task<AccountCompany> CreateAccountCompanyAsync(AccountCompany accountCompany); 
}
