using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAccountContracts.Dtos.Auth.Login;
using StockAccountContracts.Dtos.Auth.Register;
using StockAccountContracts.Interfaces.Services;

namespace StockAccountApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshAccessToken(string refreshToken)
        {
            var response = await _authService.RefreshAccessTokenAsync(refreshToken);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var response = await _authService.RegisterAsync(request);

            return StatusCode(response.StatusCode, response);
        }


        [Authorize]
        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin(RegisterRequestDto request)
        {
            var response = await _authService.RegisterAdminAsync(request);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);

            return StatusCode(response.StatusCode, response);
        }
    }
}
