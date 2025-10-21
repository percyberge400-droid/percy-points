using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository.Repository;
namespace POSPRA.Repositories.BaseRepository
{
    public class SqlServerRepository<T> : Repository<T>, IRepository<T> where T : class
    {
        public SqlServerRepository(SqlServerDbContext context) : base(context) { }

        public async Task<List<T>> ExecuteProcedureAsync<T>(string procedureName, Func<DbDataReader, T>? map = null,          // optional mapper
        SqlParameter[]? parameters = null)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
                throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));

            var conn = _context.Database.GetDbConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters is { Length: > 0 })
                cmd.Parameters.AddRange(parameters);

            var shouldClose = conn.State != ConnectionState.Open;
            if (shouldClose) await conn.OpenAsync();

            var results = new List<T>();
            try
            {
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (map != null)
                    {
                        // Case 1: Custom mapping provided
                        results.Add(map(reader));
                    }
                    else if (typeof(T) == typeof(Dictionary<string, object?>))
                    {
                        // Case 2: Auto-convert row into Dictionary<string, object?>
                        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                        results.Add((T)(object)row);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"No mapper provided for type {typeof(T).Name} and it is not a Dictionary<string, object?>."
                        );
                    }
                }
            }
            finally
            {
                if (shouldClose && conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }

            return results;
        }
    }
}
