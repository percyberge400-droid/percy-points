using Microsoft.Data.SqlClient;
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

        public async Task<List<Dictionary<string, object?>>> ExecuteProcedureToDictionaryListAsync(
            string procedureName,
            params Microsoft.Data.SqlClient.SqlParameter[] parameters)
        {
            var results = new List<Dictionary<string, object?>>();

            var conn = _context.Database.GetDbConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            bool shouldClose = conn.State != ConnectionState.Open;
            if (shouldClose)
                await conn.OpenAsync();

            try
            {
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    results.Add(row);
                }
            }
            finally
            {
                if (shouldClose && conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }

            return results;
        }

        public async Task<string?> ExecuteProcedureWithTableValuedParamAsync(
    string procedureName,
    Microsoft.Data.SqlClient.SqlParameter[] parameters,
    Microsoft.Data.SqlClient.SqlParameter? outputParameter = null)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
                throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));

            var conn = _context.Database.GetDbConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

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

        /// <summary>
        /// Executes sp_InsertPOSStatus_JSON for bulk logs insert.
        /// Returns Success/Error from @Result OUTPUT parameter.
        /// </summary>
        public async Task<string> InsertPOSStatusAsync(long posId, string logsJson)
        {
            DbConnection conn = _context.Database.GetDbConnection();
            await using var cmd = conn.CreateCommand();

            cmd.CommandText = "sp_InsertPOSStatus";
            cmd.CommandType = CommandType.StoredProcedure;

            // Parameters
            cmd.Parameters.Add(new SqlParameter("@POSID", SqlDbType.BigInt) { Value = posId });
            cmd.Parameters.Add(new SqlParameter("@LogsJson", SqlDbType.NVarChar) { Value = logsJson });

            var resultParam = new SqlParameter("@Result", SqlDbType.NVarChar, 50)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(resultParam);

            var shouldClose = conn.State != ConnectionState.Open;
            if (shouldClose)
                await conn.OpenAsync();

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return resultParam.Value?.ToString() ?? "Error";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
            finally
            {
                if (shouldClose && conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }

        }
    }
}
