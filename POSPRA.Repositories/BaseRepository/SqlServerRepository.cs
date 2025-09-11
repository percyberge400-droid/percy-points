using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository.Repository;

namespace POSPRA.Repositories.BaseRepository
{
    // 🔹 SQL Server specific repository
    public class SqlServerRepository<T> : Repository<T>, IRepository<T> where T : class
    {
        public SqlServerRepository(SqlServerDbContext context) : base(context) { }

        /// <summary>
        /// Executes a stored procedure and returns the first column
        /// of the first row as a string (or null if no result).
        /// </summary>
        public async Task<string?> ExecuteScalarProcedureAsync(
            string procedureName,
            params SqlParameter[] parameters)
        {
            await using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (var p in parameters)
                cmd.Parameters.Add(p);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }
    }
}