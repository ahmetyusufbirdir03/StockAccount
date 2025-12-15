using StockAccountDomain.Enums;

namespace StockAccountDomain.Entities;

public class ActTrans : BaseEntity
{
    public ActTrans()
    {

    }

    public ActTrans(Guid companyId, Guid accountId, Guid receiptId, decimal amount, string description)
    {
        CompanyId = companyId;
        AccountId = accountId;
        ReceiptId = receiptId;
        Amount = amount;
        Description = description;
    }
    public Guid CompanyId { get; set; }
    public Guid AccountId { get; set; }
    public Guid ReceiptId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
    public Account Account { get; set; }
    public Receipt Receipt { get; set; }
}
