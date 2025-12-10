using StockAccountDomain.Enums;

namespace StockAccountDomain.Entities;

public class AccountCompany
{
    public AccountCompany()
    {
        
    }

    public AccountCompany(Guid companyId, Guid accountId)
    {
        AccountId = accountId;
        CompanyId = companyId;
    }
    public Guid AccountId { get; set; }
    public Account Account { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
    public decimal Balance { get; set; } = 0;
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}
