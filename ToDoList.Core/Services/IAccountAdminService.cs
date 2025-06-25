using ToDoList.Core.Dtos;

namespace ToDoList.Services
{
    public interface IAccountAdminService : IAccountService
    {
        Task<IEnumerable<UserDto>> GetAllUser();
        Task<string> BlockUserAsync(string userId, int blockDateTime);
        Task<string> UnblockUserAsync(string userId);
        Task<string> DeleteUserAsync(string userId);

        Task<string> AddRoleAsync(string role);
        Task<string> AddUserToRoleAsync(string userId, string role);
    }
}
