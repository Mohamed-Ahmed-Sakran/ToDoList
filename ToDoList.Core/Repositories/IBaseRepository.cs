using System.Linq.Expressions;
using ToDoList.Core.Consts;

namespace ToDoList.Core.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> GetAsync(int Id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> FindAsync(Expression<Func<T,bool>> criteria);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T,bool>> criteria);
        Task<T> FindAsync(Expression<Func<T,bool>> criteria,params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T,bool>> criteria, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T,bool>> criteria,int skip,int take);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int? skip, int? take,
            Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddAsync(IEnumerable<T> entities);
        int Update(T entity);
        Task<T> DeleteAsync(Expression<Func<T, bool>> criteria);
    }
}
