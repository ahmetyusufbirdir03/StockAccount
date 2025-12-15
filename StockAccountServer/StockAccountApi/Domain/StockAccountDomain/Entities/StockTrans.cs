using StockAccountDomain.Enums;

namespace StockAccountDomain.Entities;

public class StockTrans : BaseEntity
{
    public StockTrans()
    {
        
    }

    public StockTrans(Guid companyId, Guid? accountId, Guid stockId, StockTransTypeEnum type, decimal quantity, decimal unitPrice, decimal totalPrice)
    {
        CompanyId = companyId;
        AccountId = accountId;
        StockId = stockId;
        Type = type;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
    }

    public Guid CompanyId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid StockId { get; set; }
    public StockTransTypeEnum Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
    public Account Account { get; set; }
    public Stock Stock { get; set; }
}
