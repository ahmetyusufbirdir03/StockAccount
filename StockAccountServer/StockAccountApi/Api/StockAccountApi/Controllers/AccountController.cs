using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAccountContracts.Dtos.Account.Create;
using StockAccountContracts.Dtos.Account.Update;
using StockAccountContracts.Interfaces.Services;

namespace StockAccountApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateAccount(CreateAccountRequestDto Request)
        {
            var result = await _accountService.CreateAccountAsync(Request);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateAccount(UpdateAccountRequestDto Request)
        {
            var result = await _accountService.UpdateAccountAsync(Request);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var result = await _accountService.GetAllAccountsAsync();
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpGet("ById/{CompanyId}")]
        public async Task<IActionResult> GetAllAccounts(Guid CompanyId)
        {
            var result = await _accountService.GetAccountsByCompanyIdAsync(CompanyId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpDelete("Delete/{AccountId}")]
        public async Task<IActionResult> DeleteAccount(Guid AccountId)
        {
            var result = await _accountService.DeleteAccountAsync(AccountId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }
    }
}
