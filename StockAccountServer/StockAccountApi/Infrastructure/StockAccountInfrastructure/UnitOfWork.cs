using Microsoft.AspNetCore.Http;
using StockAccountContracts.Interfaces;
using StockAccountContracts.Interfaces.Repositories;
using StockAccountInfrastructure.Context;
using StockAccountInfrastructure.Repositories;

namespace StockAccountInfrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UnitOfWork(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async ValueTask DisposeAsync() => await _dbContext.DisposeAsync();

    public IGenericRepository<T> GetGenericRepository<T>() where T : class, new()
    => new GenericRepository<T>(_dbContext, _httpContextAccessor);


    public int SaveChanges() => _dbContext.SaveChanges();


    public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
