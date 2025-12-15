using StockAccountDomain.Entities;

namespace StockAccountDomain.Respositories;

public interface IStockTransRepository
{
    Task<StockTrans> CreateStockTransAsync(StockTrans stockTrans);
}
