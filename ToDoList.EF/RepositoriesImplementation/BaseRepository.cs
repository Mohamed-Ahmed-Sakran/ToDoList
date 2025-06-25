using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using ToDoList.Core.Consts;
using ToDoList.Core.Repositories;
using ToDoList.EF.Data;

namespace ToDoList.EF.RepositoriesImplementation
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T> GetAsync(int Id)
        {
            return await _context.Set<T>().FindAsync(Id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return await _context.Set<T>().FirstOrDefaultAsync(criteria);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> criteria, params Expression<Func<T, object>>[] includes)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            if (includes == null)
                throw new ArgumentNullException(nameof(includes), "Include expression cannot be null.");

            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            
            return await query.FirstOrDefaultAsync(criteria);
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return await _context.Set<T>().Where(criteria).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, params Expression<Func<T, object>>[] includes)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            IQueryable<T> query = _context.Set<T>();

            if (includes is not null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.Where(criteria).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int skip, int take)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return await _context.Set<T>().Where(criteria).Skip(skip).Take(take).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int? skip, int? take, Expression<Func<T, object>> orderBy = null, string orderByDirection = "ASC")
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            IQueryable<T> query = _context.Set<T>().Where(criteria);

            if (take.HasValue)
                query = query.Take(take.Value);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (orderBy != null)
            {
                if (orderByDirection == OrderBy.Ascending)
                    query = query.OrderBy(orderBy);
                else
                    query = query.OrderByDescending(orderBy);
            }

            return await query.ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> AddAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await _context.Set<T>().AddRangeAsync(entities);
            return entities;
        }

        public int Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Set<T>().Update(entity);
            var result = _context.SaveChanges();

            return result;
        }

        public async Task<T> DeleteAsync(Expression<Func<T, bool>> criteria)
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(criteria);

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
    }
}