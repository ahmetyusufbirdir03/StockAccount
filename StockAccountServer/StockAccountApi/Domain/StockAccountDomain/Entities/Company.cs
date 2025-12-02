namespace StockAccountDomain.Entities;

public class Company : BaseEntity
{
    public Guid UserId { get; set; }
    public string? CompanyName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }


    //NAVIGATION PROPERTY
    public User User { get; set; }
    public ICollection<Account> Accounts { get; set; }
    public ICollection<Receipt> Receipts { get; set; }
    public ICollection<Stock> Stocks { get; set; }
    public ICollection<StockTrans> StockTransactions { get; set; }
    public ICollection<ActTrans> ActTransactions { get; set; }
}
