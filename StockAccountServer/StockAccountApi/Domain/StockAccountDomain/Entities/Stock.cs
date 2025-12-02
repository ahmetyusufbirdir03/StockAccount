using StockAccountDomain.Enums;

namespace StockAccountDomain.Entities;

public class Stock : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; }
    public decimal Quantity { get; set; }
    public UnitEnum Unit { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
    public ICollection<StockTrans> StockTransactions { get; set; }
}
