namespace StockAccountDomain.Models;

public class ActTransModel
{
    public Guid CompanyId { get; set; }
    public Guid AccountId { get; set; }
    public Guid ReceiptId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
