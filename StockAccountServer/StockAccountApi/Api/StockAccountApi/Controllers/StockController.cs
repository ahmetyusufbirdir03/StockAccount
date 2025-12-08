using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAccountContracts.Dtos.Stock.Create;
using StockAccountContracts.Dtos.Stock.Update;
using StockAccountContracts.Interfaces.Services;

namespace StockAccountApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllStocks()
        {
            var result = await _stockService.GetAllStocksAsync();
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpGet("CompanyStocks/{companyId}")]
        public async Task<IActionResult> GetCompanyStocks(Guid companyId)
        {
            var result = await _stockService.GetCompanyStokcsAsync(companyId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpDelete("SoftDelete/{stockId}")]
        public async Task<IActionResult> SoftDeleteStock(Guid stockId)
        {
            var result = await _stockService.SoftDeleteStockAsync(stockId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpDelete("Delete/{stockId}")]
        public async Task<IActionResult> DeleteStock(Guid stockId)
        {
            var result = await _stockService.DeleteStockAsync(stockId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateStock(UpdateStockRequestDto request)
        {
            var result = await _stockService.UpdateStockAsync(request);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateStock(CreateStockRequestDto request)
        {
            var result = await _stockService.CreateStockAsync(request);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }


    }
}
