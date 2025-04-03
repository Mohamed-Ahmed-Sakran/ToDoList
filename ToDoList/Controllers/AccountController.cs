using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Dtos;
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

        public AccountController(IAccountService accountService, IAccountAdminService accountAdminService)
        {
            _accountService = accountService;
            _accountAdminService = accountAdminService;
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

            return Ok(authResult);
        }

        //[HttpPost("Login")]
        //[MapToApiVersion("1.0")]
        //public async Task<IActionResult> LoginV1([FromBody] LoginDto loginDto)
        //{
        //    if (loginDto is null)
        //        return BadRequest("There is an error in data");

        //    var authResult = await _accountService.LoginV1Async(loginDto);

        //    if (!authResult.IsAuthenticated)
        //        return BadRequest(authResult.Message);

        //    return Ok(authResult);
        //}

        //[HttpPost("Login")]
        //[MapToApiVersion("2.0")]
        //public async Task<IActionResult> LoginV2([FromBody] LoginDto loginDto)
        //{
        //    if (loginDto is null)
        //        return BadRequest("There is an error in data");

        //    var authResult = await _accountService.LoginV2Async(loginDto);

        //    if (authResult is null)
        //        return BadRequest("Unexpected error occurred");

        //    if (!authResult.IsAuthenticated)
        //        return BadRequest(authResult.Message);

        //    return Ok(authResult);
        //}

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
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Unblock/{userId}")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
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
                return BadRequest(ex.Message);
            }
        }
    }
}
