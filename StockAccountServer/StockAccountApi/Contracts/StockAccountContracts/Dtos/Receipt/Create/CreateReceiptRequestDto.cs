using StockAccountDomain.Enums;

namespace StockAccountContracts.Dtos.Receipt.Create;

public class CreateReceiptRequestDto
{
    public Guid CompanyId { get; set; }
    public Guid AccountId { get; set; }
    public Guid StockId { get; set; }
    public decimal Quantity { get; set; }
    public ReceiptTypeEnum Type { get; set; }
    public string Description { get; set; }
}
