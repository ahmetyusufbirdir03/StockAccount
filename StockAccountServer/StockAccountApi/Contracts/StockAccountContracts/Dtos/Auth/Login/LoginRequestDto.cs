using System.ComponentModel;

namespace StockAccountContracts.Dtos.Auth.Login;

public class LoginRequestDto
{
    [DefaultValue("admin@mail")]
    public string Email { get; set; }
    [DefaultValue("123Admin")]
    public string Password { get; set; }
}
