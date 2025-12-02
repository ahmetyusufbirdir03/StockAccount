using StockAccountContracts.Interfaces.Repositories;

namespace StockAccountContracts.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<T> GetGenericRepository<T>() where T : class, new();
    Task<int> SaveChangesAsync(); //asenkron
    int SaveChanges(); // senkron

}
