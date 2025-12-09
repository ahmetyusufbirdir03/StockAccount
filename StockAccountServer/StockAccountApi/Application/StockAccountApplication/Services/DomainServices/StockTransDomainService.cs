using StockAccountDomain.Entities;
using StockAccountDomain.Models;
using StockAccountDomain.Respositories;
using StockAccountDomain.Services;

namespace StockAccountApplication.Services.DomainServices;

public class StockTransDomainService : IStockTransDomainService
{
    private readonly IStockTransRepository _stockTransRepository;
    public StockTransDomainService(IStockTransRepository stockTransRepository)
    {
        _stockTransRepository = stockTransRepository;
    }
    public async Task<StockTrans> CreateStockTransAsync(StockTransModel stockTransModel)
    {
        var stockTrans = new StockTrans(
            stockTransModel.CompanyId, stockTransModel.CounterpartyCompanyId, stockTransModel.StockId,
            stockTransModel.Type, stockTransModel.Quantity, stockTransModel.UnitPrice,
            stockTransModel.TotalPrice);
        stockTrans.CreatedAt = DateTime.UtcNow;

        var responseData = await _stockTransRepository.CreateStockTransAsync(stockTrans);

        return responseData;
    }
}
