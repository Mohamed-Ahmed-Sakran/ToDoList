using System;

namespace ToDoList.Core.Repositories
{
    public interface IUserRepository
    {
        public Task IsUserFoundAsync(string userId);
    }
}
