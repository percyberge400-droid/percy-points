using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository.Repository;

namespace POSPRA.Repositories.BaseRepository
{
    /// <summary>
    ///     SQL Server–specific generic repository.
    ///     <para>
    ///     In addition to the base <see cref="Repository{T}"/> CRUD methods,
    ///     this class provides a helper to execute a stored procedure that
    ///     returns a single scalar value through an output parameter.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">
    ///     Entity type managed by the repository. The generic type is not used
    ///     by <see cref="ExecuteScalarProcedureWithOutputAsync"/> but allows the
    ///     repository to participate in the same DI/Repository pattern as the base class.
    /// </typeparam>
    public class SqlServerRepository<T> : Repository<T>, IRepository<T> where T : class
    {
        public SqlServerRepository(SqlServerDbContext context) : base(context) { }

        /// <summary>
        /// Executes a stored procedure that may include input parameters
        /// and an optional output parameter, and returns the output value
        /// as a string.
        /// </summary>
        /// <param name="procedureName">
        /// Name of the stored procedure to execute.
        /// </param>
        /// <param name="inputParameters">
        /// Array of input <see cref="Microsoft.Data.SqlClient.SqlParameter"/> objects
        /// to pass to the procedure. Pass an empty array if none are required.
        /// </param>
        /// <param name="outputParameter">
        /// Optional output <see cref="Microsoft.Data.SqlClient.SqlParameter"/> whose
        /// value will be retrieved after execution. If null, the method
        /// simply executes the procedure without returning a value.
        /// </param>
        /// <returns>
        /// The string value of the output parameter if provided and set;
        /// otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="procedureName"/> is null or empty.
        /// </exception>
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