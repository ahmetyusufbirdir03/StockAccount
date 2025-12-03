using StockAccountDomain.Entities;

namespace StockAccountContracts.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByPhoneNumberAsync(string phoneNumber);
    }
}
