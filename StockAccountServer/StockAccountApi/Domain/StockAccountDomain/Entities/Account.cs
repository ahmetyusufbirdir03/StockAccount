using StockAccountDomain.Enums;

namespace StockAccountDomain.Entities;

public class Account : BaseEntity
{
    public string? AccountName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }


    //NAVIGATION PROPERTY
    public ICollection<AccountCompany> AccountCompanies { get; set; }
    public ICollection<Receipt> Receipts { get; set; }
    public ICollection<ActTrans> ActTransactions { get; set; }
}
