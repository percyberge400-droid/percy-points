using Microsoft.Data.Sqlite;
using Pos.Application.DTOs.DatabaseMigrationDtos;
using Pos.Application.Utility.DatabaseMigrationHelper;

namespace Pos.Infrastructure.Services.DatabaseMigrationService
{
    public class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly string _connectionString;
        private readonly SqliteDbContext _context;

        private readonly SchemaReader _schemaReader;
        private readonly SchemaApplier _schemaApplier;

        public DatabaseMigrationService(
            string connectionString,
            SqliteDbContext context)
        {
            _connectionString = connectionString;
            _context = context;

            _schemaReader = new SchemaReader();
            _schemaApplier = new SchemaApplier();
        }

        public async Task ApplyMigrationsAsync()
        {
            using SqliteConnection connection = new(_connectionString);
            await connection.OpenAsync();

            Dictionary<string, List<ColumnInfo>> existingSchema =
                await _schemaReader.ReadExistingSchemaAsync(connection);

            Dictionary<string, List<ColumnInfo>> desiredSchema =
                _schemaReader.ReadDesiredSchema(_context);

            await _schemaApplier.AddMissingTablesAndColumnsAsync(
                connection,
                existingSchema,
                desiredSchema);
        }
    }
}
