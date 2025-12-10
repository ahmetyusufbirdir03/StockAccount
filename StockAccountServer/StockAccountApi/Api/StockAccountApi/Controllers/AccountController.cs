using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAccountApplication.Services;
using StockAccountContracts.Dtos.Account.Create;
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
    }
}
