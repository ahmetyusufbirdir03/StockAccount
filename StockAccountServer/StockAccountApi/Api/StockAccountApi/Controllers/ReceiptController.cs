using Microsoft.AspNetCore.Mvc;
using StockAccountContracts.Dtos;
using StockAccountContracts.Dtos.Receipt;
using StockAccountContracts.Dtos.Receipt.Create;
using StockAccountContracts.Interfaces.Services;

namespace StockAccountApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReceiptController : ControllerBase
{
    private readonly IReceiptService _receiptService;

    public ReceiptController(IReceiptService receiptService)
    {
        _receiptService = receiptService;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateReceipt(CreateReceiptRequestDto request)
    {
        var result = await _receiptService.CreateReceiptAsync(request);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result);
        }
        return Ok(result);
    }

    [HttpDelete("Delete/{ReceiptId}")]
    public async Task<IActionResult> DeleteReceipt(Guid ReceiptId)
    {
        var result = await _receiptService.DeleteReceiptAsync(ReceiptId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result);
        }
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetReceipts(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? stockId)
    {
        ResponseDto<IList<ReceiptResponseDto>> result;

        if (companyId.HasValue && accountId.HasValue && stockId.HasValue)
        {
            result = await _receiptService
                .GetCompanyReceiptsByAccountIdAndStockIdAsync(
                    companyId.Value,
                    accountId.Value,
                    stockId.Value);
        }
        else if (companyId.HasValue && accountId.HasValue)
        {
            result = await _receiptService
                .GetCompanyReceiptsByAccountIdAsync(
                    companyId.Value,
                    accountId.Value);
        }
        else if (companyId.HasValue)
        {
            result = await _receiptService
                .GetReceiptsByCompanyIdAsync(companyId.Value);
        }
        else
        {
            result = await _receiptService.GetAllReceiptsAsync();
        }

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}
