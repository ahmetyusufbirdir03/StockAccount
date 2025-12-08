using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Stock;
using StockAccountContracts.Dtos.Stock.Create;
using StockAccountContracts.Dtos.Stock.Update;

namespace StockAccountContracts.Interfaces.Services;

public interface IStockService
{
    /// <summary>
    ///     Delete stock permanently, admin function.
    /// </summary>
    /// <param name="StockId"></param>
    /// <returns></returns>
    Task<ResponseDto<NoContentDto>> DeleteStockAsync(Guid StockId);

    /// <summary>
    ///     Mars stocks as deleted by changing DeletedAt and DeletedBy.
    /// </summary>
    /// <param name="StockId"></param>
    /// <returns></returns>
    Task<ResponseDto<NoContentDto>> SoftDeleteStockAsync(Guid StockId);

    /// <summary>
    ///     Gets all stocks, admin function.
    /// </summary>
    /// <returns></returns>
    Task<ResponseDto<IList<StockResponseDto>>> GetAllStocksAsync();

    /// <summary>
    ///     Gets stocks of user
    /// </summary>
    /// <param name="UserId"></param>
    /// <returns></returns>
    Task<ResponseDto<IList<StockResponseDto>>> GetCompanyStokcsAsync(Guid CompanyId);

    /// <summary>
    ///     Creates a new stock
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    Task<ResponseDto<StockResponseDto>> 
        CreateStockAsync(CreateStockRequestDto Request);

    /// <summary>
    ///     Updates stock information
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    Task<ResponseDto<StockResponseDto>> 
        UpdateStockAsync(UpdateStockRequestDto Request);
}
