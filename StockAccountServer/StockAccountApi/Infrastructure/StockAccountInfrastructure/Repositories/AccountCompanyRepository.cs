using Microsoft.EntityFrameworkCore;
using StockAccountDomain.Entities;
using StockAccountDomain.Respositories;
using StockAccountInfrastructure.Context;

namespace StockAccountInfrastructure.Repositories;

public class AccountCompanyRepository : IAccountCompanyRepository
{
    private readonly AppDbContext _dbContext;
    protected readonly DbSet<AccountCompany> _dbSet;

    public AccountCompanyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<AccountCompany>();
    }
    public async Task<AccountCompany> CreateAccountCompanyAsync(AccountCompany accountCompany)
    {
        await _dbSet.AddAsync(accountCompany);
        await _dbContext.SaveChangesAsync();
        return accountCompany;
    }
}
