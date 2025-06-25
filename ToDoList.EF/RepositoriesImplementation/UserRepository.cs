using Microsoft.EntityFrameworkCore;
using System;
using ToDoList.Core.Repositories;
using ToDoList.EF.Data;

namespace ToDoList.EF.RepositoriesImplementation
{
    class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task IsUserFoundAsync(string userId)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
                throw new Exception("There is no user !");
        }
    }
}
