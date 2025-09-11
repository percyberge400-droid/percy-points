using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace POSPRA.Repositories.BaseRepository.Repository
{
    // 🔹 Generic repository base class
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(object id) =>
            await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity) =>
            await _dbSet.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities) =>
            await _dbSet.AddRangeAsync(entities);

        public void Update(T entity) =>
            _dbSet.Update(entity);

        public void Remove(T entity) =>
            _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) =>
            _dbSet.RemoveRange(entities);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<int> ExecuteProcedureAsync(string procedureName, params object[] parameters)
        {
            // Use ExecuteSqlRawAsync if you are passing a plain SQL string
            // or ExecuteSqlInterpolatedAsync if you want interpolation.
            return await _context.Database.ExecuteSqlRawAsync(procedureName, parameters);
        }

        public async Task<List<TResult>> QueryProcedureAsync<TResult>(string procedureName, params object[] parameters) where TResult : class
        {
            // TResult must be registered in the DbContext model (entity) or be a keyless type.
            return await _context.Set<TResult>().FromSqlRaw(procedureName, parameters).ToListAsync();
        }
    }
}