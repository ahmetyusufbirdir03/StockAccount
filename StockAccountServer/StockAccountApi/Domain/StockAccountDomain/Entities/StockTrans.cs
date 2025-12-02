using StockAccountDomain.Enums;

namespace StockAccountDomain.Entities;

public class StockTrans : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid StockId { get; set; }
    public StockTransTypeEnum Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Description { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
    public Stock Stock { get; set; }
}
