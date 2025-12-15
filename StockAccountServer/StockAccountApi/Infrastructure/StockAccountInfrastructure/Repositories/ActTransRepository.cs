using Microsoft.EntityFrameworkCore;
using StockAccountDomain.Entities;
using StockAccountDomain.Respositories;
using StockAccountInfrastructure.Context;

namespace StockAccountInfrastructure.Repositories;

public class ActTransRepository : IActTransRepository
{
    private readonly AppDbContext _dbContext;
    protected readonly DbSet<ActTrans> _dbSet;

    public ActTransRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<ActTrans>();
    }
    public async Task<ActTrans> CreateActTransAsync(ActTrans actTrans)
    {
        await _dbSet.AddAsync(actTrans);
        await _dbContext.SaveChangesAsync();
        return actTrans;
    }
}
