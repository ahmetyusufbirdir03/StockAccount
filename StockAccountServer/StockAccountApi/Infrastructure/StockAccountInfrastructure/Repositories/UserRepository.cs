using Microsoft.EntityFrameworkCore;
using StockAccountContracts.Interfaces.Repositories;
using StockAccountDomain.Entities;
using StockAccountInfrastructure.Context;

namespace StockAccountApplication.Repositories
{
    
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;
        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);   
            return user;   
        }
    }
}
