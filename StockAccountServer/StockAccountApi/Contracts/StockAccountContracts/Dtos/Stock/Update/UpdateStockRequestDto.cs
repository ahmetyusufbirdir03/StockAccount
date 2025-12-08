namespace StockAccountContracts.Dtos.Stock.Update;

public class UpdateStockRequestDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

