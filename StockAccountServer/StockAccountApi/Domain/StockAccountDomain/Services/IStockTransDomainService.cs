using StockAccountDomain.Entities;
using StockAccountDomain.Models;

namespace StockAccountDomain.Services
{
    public interface IStockTransDomainService
    {
        Task<StockTrans> CreateStockTransAsync(StockTransModel stockTransModel);
    }
}
