using Microsoft.EntityFrameworkCore;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository.Repository;
using System.Data;
using System.Data.Common;
namespace POSPRA.Repositories.BaseRepository
{
    public class SqlServerRepository<T> : Repository<T>, IRepository<T> where T : class
    {
        public SqlServerRepository(SqlServerDbContext context) : base(context) { }

        public async Task<string?> ExecuteScalarProcedureWithOutputAsync(
            string procedureName,
            Microsoft.Data.SqlClient.SqlParameter[] inputParameters,
            Microsoft.Data.SqlClient.SqlParameter? outputParameter = null)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
                throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));

            DbConnection conn = _context.Database.GetDbConnection();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;

            if (inputParameters != null && inputParameters.Length > 0)
                cmd.Parameters.AddRange(inputParameters);

            if (outputParameter != null)
                cmd.Parameters.Add(outputParameter);

            bool shouldClose = conn.State != ConnectionState.Open;
            if (shouldClose)
                await conn.OpenAsync();

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return outputParameter?.Value?.ToString();
            }
            finally
            {
                if (shouldClose && conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }
    }
}
