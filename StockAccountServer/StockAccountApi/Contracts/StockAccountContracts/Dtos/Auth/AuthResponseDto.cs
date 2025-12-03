namespace StockAccountContracts.Dtos.Auth;

public class AuthResponseDto
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
}
