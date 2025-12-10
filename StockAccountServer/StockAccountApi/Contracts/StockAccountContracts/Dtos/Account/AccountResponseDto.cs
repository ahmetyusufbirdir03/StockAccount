using StockAccountDomain.Enums;

namespace StockAccountContracts.Dtos.Account;

public class AccountResponseDto
{
    public string? AccountName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
