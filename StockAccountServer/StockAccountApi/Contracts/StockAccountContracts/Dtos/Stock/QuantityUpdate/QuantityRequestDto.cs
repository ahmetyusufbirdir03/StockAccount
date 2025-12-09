namespace StockAccountContracts.Dtos.Stock.QuantityUpdate;

public class QuantityRequestDto
{
    public Guid StockId { get; set; }
    public decimal Amount { get; set; }
    public bool IsAddition { get; set; } = false;
}
