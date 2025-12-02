using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using StockAccountContracts.Interfaces.Repositories;
using StockAccountInfrastructure.Context;
using System.Linq.Expressions;

namespace StockAccountInfrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class, new()
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _dbSet = _dbContext.Set<T>();
    }
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<T>> GetAllAsync(
    Expression<Func<T, bool>>? predicate = null,
    Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
            query = query.Where(predicate);

        if (include != null)
            query = include(query);

        return await query.ToListAsync();
    }


    public async Task<T?> GetByIdAsync(Guid Id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == Id);
    }

    public async Task SoftDeleteAsync(T entity)
    {
        var deletedByProp =
            typeof(T).GetProperty("DeletedBy") ??
            throw new InvalidOperationException
            ("Entity doesn't have DeletedBy property");


        if (deletedByProp.GetValue(entity) != null)
            throw new Exception("Entity is already deleted!");

        var deletedBy = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        deletedByProp.SetValue(entity, deletedBy);

        var deletedDateProp =
            typeof(T).GetProperty("DeletedDate") ??
            throw new InvalidOperationException
            ("Entity doesn't have DeletedDate property");

        deletedDateProp?.SetValue(entity, DateTime.UtcNow);

        _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await _dbContext.SaveChangesAsync();
    }
}
