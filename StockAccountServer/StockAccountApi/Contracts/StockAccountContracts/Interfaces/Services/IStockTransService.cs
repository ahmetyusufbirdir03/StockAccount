using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.StockTrans;
using StockAccountContracts.Dtos.StockTrans.Create;

namespace StockAccountContracts.Interfaces.Services;

public interface IStockTransService
{
    /// <summary>
    ///     Delete stock transaction permenantly, admin function.
    /// </summary>
    /// <param name="StockTransId"></param>
    /// <returns></returns>
    Task<ResponseDto<NoContentDto>> DeleteStockTransactionAsync(Guid StockTransId);

    /// <summary>
    /// Gets all stock transactions (Admin function).
    /// </summary>
    Task<ResponseDto<IList<StockTransResponseDto>>> GetAllStockTransactionsAsync();

    /// <summary>
    /// Gets all transactions of a specific stock.
    /// </summary>
    /// <param name="stockId">The ID of the stock.</param>
    Task<ResponseDto<IList<StockTransResponseDto>>> GetTransactionsByStockIdAsync(Guid stockId);

    /// <summary>
    ///   Creates a stock transaction.
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    Task<ResponseDto<StockTransResponseDto>> CreateStockTransactionAsync(CreateStockTransRequestDto Request);
}
