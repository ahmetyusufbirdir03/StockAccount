using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAccountContracts.Dtos.StockTrans.Create;
using StockAccountContracts.Interfaces.Services;

namespace StockAccountApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockTransactionController : ControllerBase
    {
        private readonly IStockTransService _stockTransService;
        public StockTransactionController(IStockTransService stockTransService)
        {
            _stockTransService = stockTransService;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var result = await _stockTransService.GetAllStockTransactionsAsync();
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpGet("StockTransactions/{StockId}")]
        public async Task<IActionResult> GetTransactionsByStockId(Guid StockId)
        {
            var result = await _stockTransService.GetTransactionsByStockIdAsync(StockId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpDelete("{StockTransId}")]
        public async Task<IActionResult> DeleteStockTransaction(Guid StockTransId)
        {
            var result = await _stockTransService.DeleteStockTransactionAsync(StockTransId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStockTrans(CreateStockTransRequestDto Request)
        {
            var result = await _stockTransService.CreateStockTransactionAsync(Request);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }
    }
}
