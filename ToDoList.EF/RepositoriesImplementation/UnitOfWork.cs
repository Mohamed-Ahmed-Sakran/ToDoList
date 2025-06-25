using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using ToDoList.Core.Models;
using ToDoList.Core.Repositories;
using ToDoList.EF.Data;

namespace ToDoList.EF.RepositoriesImplementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private AppDbContext _context;

        public IMissionRepository missions { get; private set; }
        public IUserRepository users { get; private set; }

        public UnitOfWork(AppDbContext context, UserManager<AppUser> userManager, IMapper mapper)
        {
            _context = context;
            missions = new MissionRepository(_context, userManager, mapper);
            users = new UserRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
