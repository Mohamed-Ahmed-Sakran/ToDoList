using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoList.Data;
using ToDoList.Dtos;
using ToDoList.Helpers;
using ToDoList.Models;

namespace ToDoList.Services
{
    public class AccountAdminService : AccountService , IAccountAdminService
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IMapper _mapper;

        private readonly JwtOption _jwtOption;

        public AccountAdminService(UserManager<AppUser> userManager, IMapper mapper, JwtOption jwtOption, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor) : base(userManager, mapper, jwtOption, httpContextAccessor)
        {
            _userManager = userManager;
            _mapper = mapper;
            _jwtOption = jwtOption;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<UserDto>> GetAllUser()
        {
            var usersFromDb = await _userManager.Users.Include(x => x.Missions).ToListAsync();

            if (usersFromDb is null || !usersFromDb.Any())
                throw new Exception("No users found");

            var users = _mapper.Map<IEnumerable<UserDto>>(usersFromDb);

            return users;
        }

        public async Task<string> BlockUserAsync(string userId , int blockDateTime)
        {
            if (userId is null || blockDateTime <= 0)
                throw new ArgumentException("There is an error in data"); ;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("There is no user with this userId");

            user.BlockUntil = DateTime.Now.AddDays(blockDateTime);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to block user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            return "User is blocked";
        }

        public async Task<string> UnblockUserAsync(string userId)
        {
            if (userId is null)
                throw new ArgumentException("There is an error in data"); ;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("There is no user with this userId");

            if (!user.BlockUntil.HasValue || user.BlockUntil <= DateTime.Now)
                return "User doesn't block ";

            user.BlockUntil = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to unblock user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            return "User is unblocked , Welcome ";
        }

        public async Task<string> DeleteUserAsync(string userId)
        {
            if (userId is null)
                throw new ArgumentException("There is an error in data"); ;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("There is no user with this userId");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to delete user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            return "User is deleted";
        }

        public async Task<string> AddRoleAsync(string role)
        {
            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException("There is an error in data");

            if (await _roleManager.RoleExistsAsync(role))
                throw new Exception("Role is already exist !");

            var newRole = new IdentityRole(role)
            {
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var result = await _roleManager.CreateAsync(newRole);

            if (!result.Succeeded)
            {
                var errors = "Failed to create role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            return "Role added successfully";
        }

        public async Task<string> AddUserToRoleAsync(string userId, string role)
        {
            if (role is null || userId is null)
                throw new ArgumentNullException("There is an error in data");

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null || !await _roleManager.RoleExistsAsync(role))
                throw new Exception("User or Role doesn't exist !");

            if (await _userManager.IsInRoleAsync(user, role))
                throw new Exception("User is already sign in this role");

            var result = await _userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded)
            {
                var errors = "Failed to Add user to the role: ";
                foreach (var error in result.Errors)
                {
                    errors += string.Join(",",error.Description);
                }
                throw new Exception(errors);
            }

            return "User added successfully to the role";
        }
    }
}
