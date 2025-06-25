using System;

namespace ToDoList.Core.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IMissionRepository missions { get; }
        IUserRepository users { get; }

        Task<int> CompleteAsync();
    }
}
