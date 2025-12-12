using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Receipt;
using StockAccountContracts.Dtos.Receipt.Create;

namespace StockAccountContracts.Interfaces.Services;

public interface IReceiptService
{
    /// <summary>
    /// Deletes a receipt by its unique identifier.
    /// </summary>
    /// <param name="receiptId">The unique identifier of the receipt to be deleted.</param>
    /// <returns>
    /// A response indicating the result of the deletion process.
    /// Returns <see cref="NoContentDto"/> on success.
    /// </returns>
    Task<ResponseDto<NoContentDto>> DeleteReceiptAsync(Guid receiptId);


    /// <summary>
    /// Issues an invoice by creating a new receipt based on the provided request data.
    /// </summary>
    /// <param name="request">The request model containing the receipt creation details.</param>
    /// <returns>
    /// A response containing the created receipt information,
    /// or an error response if the creation fails.
    /// </returns>
    Task<ResponseDto<ReceiptResponseDto>> CreateReceiptAsync(CreateReceiptRequestDto request);


    /// <summary>
    /// Retrieves all receipts in the system.  
    /// This method is intended for administrative use.
    /// </summary>
    /// <returns>
    /// A response containing the full list of receipts,
    /// or an error response if the user is not authorized.
    /// </returns>
    Task<ResponseDto<IList<ReceiptResponseDto>>> GetAllReceiptsAsync();


    /// <summary>
    /// Retrieves all receipts issued by a company for a specific account.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <param name="accountId">The unique identifier of the account whose receipts will be retrieved.</param>
    /// <returns>
    /// A response containing the list of receipts filtered by company and account,
    /// or an error response if the company or account is invalid.
    /// </returns>
    Task<ResponseDto<IList<ReceiptResponseDto>>> GetCompanyReceiptsByAccountIdAsync(Guid companyId, Guid accountId);


    /// <summary>
    /// Retrieves all receipts belonging to a specific company.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <returns>
    /// A response containing the list of company receipts,
    /// or an error response if the company is not found.
    /// </returns>
    Task<ResponseDto<IList<ReceiptResponseDto>>> GetReceiptsByCompanyIdAsync(Guid companyId);


    /// <summary>
    /// Gets all receipts of a specific company that were issued to a specific account
    /// and filtered by the given stock identifier.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company.</param>
    /// <param name="accountId">The unique identifier of the account that received the receipts.</param>
    /// <param name="stockId">The unique identifier of the stock used when issuing the receipts.</param>
    /// <returns>
    /// A response containing a list of receipt DTOs filtered by company, account, and stock;
    /// or an error response if the request is invalid.
    /// </returns>
    Task<ResponseDto<IList<ReceiptResponseDto>>> GetCompanyReceiptsByAccountIdAndStockIdAsync(
        Guid companyId,
        Guid accountId,
        Guid stockId);

}
