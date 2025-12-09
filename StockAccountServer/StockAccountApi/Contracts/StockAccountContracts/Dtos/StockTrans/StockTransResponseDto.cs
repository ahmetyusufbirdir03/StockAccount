using StockAccountDomain.Enums;

namespace StockAccountContracts.Dtos.StockTrans;

public class StockTransResponseDto
{
    public Guid CompanyId { get; set; }
    public Guid? CounterpartyCompanyId { get; set; }
    public Guid StockId { get; set; }
    public StockTransTypeEnum Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Description { get; set; }
}
