using System.Linq.Expressions;

namespace Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Basic CRUD Operations
        Task<T?> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(object id);
        Task DeleteAsync(T entity);

        // Query Operations
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        // Pagination
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<T>> GetPagedAsync(
            Expression<Func<T, bool>>? filter,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
            int pageNumber,
            int pageSize);

        // Save Changes
        Task<int> SaveChangesAsync();
    }
}