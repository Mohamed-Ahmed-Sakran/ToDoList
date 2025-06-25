using ToDoList.Core.Dtos;

namespace ToDoList.Services
{
    public interface IAccountService
    {
        public Task<object> LoginAsync(LoginDto LoginRequest);
        public Task<AuthDto> RegisterV1Async(RegisterDto RegisterRequest);
        public Task<RegisterResponseDto> RegisterV2Async(RegisterDto RegisterRequest);
        public Task<UserDto> UpdateAsync(string userId, UpdateUserDto updateRequest);
        public Task<LoginResponseDto> RefreshTokenAsync(string token);
        public Task<bool> RevokeTokenAsync(string token);
    }
}
