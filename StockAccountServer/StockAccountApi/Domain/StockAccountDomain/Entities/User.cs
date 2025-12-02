using Microsoft.AspNetCore.Identity;

namespace StockAccountDomain.Entities;

public class User : IdentityUser<Guid>
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string FullName => $"{Name} {Surname}".Trim();
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public ICollection<Company> Companies { get; set; } = new List<Company>();

}
