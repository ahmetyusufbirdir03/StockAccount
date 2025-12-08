using Microsoft.AspNetCore.Mvc;
using StockAccountContracts.Dtos.User.Update;
using StockAccountContracts.Interfaces.Services;
using StockAccountDomain.Entities;

namespace StockAccountApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsers();
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpDelete("SoftDelete/{userId}")]
        public async Task<IActionResult> SoftDeleteUser(Guid userId)
        {
            var result = await _userService.SoftDeleteUserAsync(userId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpDelete("Delete/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var result = await _userService.DeleteUserAsync(userId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateUser(UpdateUserRequestDto request)
        {
            var result = await _userService.UpdateUserAsync(request);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result);
            }
            return Ok(result);
        }
    }
}
