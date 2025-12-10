namespace StockAccountContracts.Dtos.Account.Create;

public class CreateAccountRequestDto
{
    public Guid CompanyId { get; set; }
    public string AccountName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
}
