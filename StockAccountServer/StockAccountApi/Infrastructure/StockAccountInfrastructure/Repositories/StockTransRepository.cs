using Microsoft.EntityFrameworkCore;
using StockAccountDomain.Entities;
using StockAccountDomain.Respositories;
using StockAccountInfrastructure.Context;

namespace StockAccountInfrastructure.Repositories;

public class StockTransRepository : IStockTransRepository
{
    private readonly AppDbContext _dbContext;
    protected readonly DbSet<StockTrans> _dbSet;

    public StockTransRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<StockTrans>();
    }
    public async Task<StockTrans> CreateStockTransAsync(StockTrans stockTrans)
    {
        await _dbSet.AddAsync(stockTrans);
        await _dbContext.SaveChangesAsync();
        return stockTrans;
    }
}
