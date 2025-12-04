namespace StockAccountContracts.Dtos.Company.Update;

public class UpdateCompanyRequestDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
}
