namespace StockAccountDomain.Entities;

public class Account : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string? AccountName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal? Balance { get; set; }

    //NAVIGATION PROPERTY
    public Company Company { get; set; }
    public ICollection<Receipt> Receipts { get; set; }
    public ICollection<ActTrans> ActTransactions { get; set; }
}
