using StockAccountDomain.Enums;

namespace StockAccountContracts.Dtos.Receipt;

public class ReceiptResponseDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid AccountId { get; set; }
    public Guid StockId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCurrentPrice { get; set; }
    public ReceiptTypeEnum Type { get; set; }
    public decimal TotalAmount { get; set; }
    public string Description { get; set; }
}
