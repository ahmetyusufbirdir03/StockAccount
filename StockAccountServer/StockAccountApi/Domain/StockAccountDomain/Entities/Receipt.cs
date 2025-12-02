using StockAccountDomain.Enums;

namespace StockAccountDomain.Entities;

public class Receipt : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid AccountId { get; set; }
    public ReceiptTypeEnum Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
    public Account Account { get; set; }
    public ICollection<ActTrans> ActTransactions { get; set; }
}
