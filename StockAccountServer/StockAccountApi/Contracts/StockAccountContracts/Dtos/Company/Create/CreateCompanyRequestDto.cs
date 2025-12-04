namespace StockAccountContracts.Dtos.Company.Create;

public class CreateCompanyRequestDto
{
    public Guid UserId { get; set; }
    public string? CompanyName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
