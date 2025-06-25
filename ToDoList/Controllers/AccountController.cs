using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoList.Core.Dtos;
using ToDoList.Services;

namespace ToDoList.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAccountAdminService _accountAdminService;
        private readonly ILogger<AccountController> logger;

        public AccountController(IAccountService accountService, IAccountAdminService accountAdminService,ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _accountAdminService = accountAdminService;
            this.logger = logger;
        }

        [Authorize(Roles = "Moderator")]
        [HttpGet]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetAllUser()
        {
            try
            {
                var users = await _accountAdminService.GetAllUser();
                return Ok(users);
            }
            catch (Exception ex)
            {
                var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                logger.LogError("There is an error when account with id: {id} try to get all participate accounts", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Register")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> RegisterV1([FromBody] RegisterDto registerDto)
        {
            if (registerDto is null)
                return BadRequest("There is an error in data");

            var registerResult = await _accountService.RegisterV1Async(registerDto);

            if (!registerResult.IsRegister)
                return BadRequest(registerResult.Message);

            return Ok(registerResult);
        }

        [HttpPost("Register")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> RegisterV2([FromBody] RegisterDto registerDto)
        {
            if (registerDto is null)
                return BadRequest("There is an error in data");

            var registerResult = await _accountService.RegisterV2Async(registerDto);

            if (!registerResult.IsRegister)
                return BadRequest(registerResult.Message);

            return Ok(registerResult);
        }

        [HttpPost("Login")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> LoginV1([FromBody] LoginDto loginDto)
        {
            if (loginDto is null)
                return BadRequest("There is an error in data");

            var authResult = await _accountService.LoginAsync(loginDto) as AuthDto;

            if (!authResult.IsAuthenticated)
                return BadRequest(authResult.Message);

            return Ok(authResult);
        }

        [HttpPost("Login")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> LoginV2([FromBody] LoginDto loginDto)
        {
            if (loginDto is null)
                return BadRequest("There is an error in data");

            var authResult = await _accountService.LoginAsync(loginDto) as LoginResponseDto;

            if (authResult is null)
                return BadRequest("Unexpected error occurred");

            if (!authResult.IsAuthenticated)
                return BadRequest(authResult.Message);

            if (!string.IsNullOrEmpty(authResult.RefreshToken))
                setRefreshTokenInCookie(authResult.RefreshToken, authResult.RefreshTokenExpiration);

            return Ok(authResult);
        }

        [Authorize(Roles = "Moderator")]
        [HttpPost("Roles")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> AddRole([FromBody] string role)
        {
            if (role is null)
                return BadRequest("There is an error in data");

            try
            {
                var r = await _accountAdminService.AddRoleAsync(role);
                return Ok(r);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Moderator,Admin")]
        [HttpPost("Roles/{userId}")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> AddUserToRole(string userId , [FromBody] string role)
        {
            if (role is null || userId is null)
                return BadRequest("There is an error in data");

            try
            {
                var r = await _accountAdminService.AddUserToRoleAsync(userId, role);
                return Ok(r);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Update/{userId}")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> Update(string userId , [FromBody] UpdateUserDto updateUserDto)
        {
            if (updateUserDto is null || userId is null)
                return BadRequest("There is an error in data");

            try
            {
                var updateResult = await _accountService.UpdateAsync(userId, updateUserDto);
                return Ok(updateResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Block/{userId}/{NumOfDayBlock}")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> BlockUser(string userId , int NumOfDayBlock)
        {
            if (string.IsNullOrEmpty(userId) || NumOfDayBlock <= 0)
                return BadRequest("There is an error in data");

            try
            {
                var result = await _accountAdminService.BlockUserAsync(userId, NumOfDayBlock);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Unblock/{userId}")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("There is an error in data");

            try
            {
                var result = await _accountAdminService.UnblockUserAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Delete/{userId}")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("There is an error in data");

            try
            {
                var result = await _accountAdminService.DeleteUserAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _accountService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            setRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto modelDto)
        {
            var Token = modelDto.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(Token))
                return BadRequest("Token is required");

            var result = await _accountService.RevokeTokenAsync(Token);

            if(!result)
                return BadRequest("Token is invalid");

            return Ok();
        }

        private void setRefreshTokenInCookie(string refreshToken , DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime()
            };

            Response.Cookies.Append("refreshToken",refreshToken, cookieOptions);
        }

    }
}
