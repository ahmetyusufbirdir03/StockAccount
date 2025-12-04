namespace StockAccountContracts.Dtos.Company;

public class CompanyResponseDto
{
    public Guid Id { get; set; }
    public string? CompanyName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
