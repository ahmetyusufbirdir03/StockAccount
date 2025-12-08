using StockAccountDomain.Enums;

namespace StockAccountContracts.Dtos.Stock;

public class StockResponseDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string? Name { get; set; }
    public decimal Quantity { get; set; }
    public UnitEnum Unit { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
