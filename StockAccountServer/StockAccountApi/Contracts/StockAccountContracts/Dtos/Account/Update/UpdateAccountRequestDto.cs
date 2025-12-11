namespace StockAccountContracts.Dtos.Account.Update;

public class UpdateAccountRequestDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string? AccountName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    public decimal Balance { get; set; }
}
