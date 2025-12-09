using StockAccountDomain.Enums;

namespace StockAccountDomain.Entities;

public class StockTrans : BaseEntity
{
    public StockTrans()
    {
        
    }

    public StockTrans(Guid companyId, Guid? counterpartyCompanyId, Guid stockId, StockTransTypeEnum type, decimal quantity, decimal unitPrice, decimal totalPrice)
    {
        CompanyId = companyId;
        CounterpartyCompanyId = counterpartyCompanyId;
        StockId = stockId;
        Type = type;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
    }

    public Guid CompanyId { get; set; }
    public Guid? CounterpartyCompanyId { get; set; }
    public Guid StockId { get; set; }
    public StockTransTypeEnum Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
    public Company CounterpartyCompany { get; set; }
    public Stock Stock { get; set; }
}
