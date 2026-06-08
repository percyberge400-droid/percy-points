using Microsoft.Data.Sqlite;
using Pos.Application.DTOs.DatabaseMigrationDtos;

namespace Pos.Application.Utility.DatabaseMigrationHelper
{
    public class SchemaApplier
    {
        public async Task AddMissingTablesAndColumnsAsync(
            SqliteConnection connection,
            Dictionary<string, List<ColumnInfo>> existingSchema,
            Dictionary<string, List<ColumnInfo>> desiredSchema)
        {
            foreach ((string? tableName, List<ColumnInfo>? desiredColumns) in desiredSchema)
            {
                if (!existingSchema.ContainsKey(tableName))
                {
                    await CreateTableAsync(connection, tableName, desiredColumns);
                }
                else
                {
                    await AddMissingColumnsAsync(
                        connection,
                        tableName,
                        existingSchema[tableName],
                        desiredColumns);
                }
            }
        }

        private async Task CreateTableAsync(
            SqliteConnection connection,
            string tableName,
            List<ColumnInfo> columns)
        {
            string columnDefs = string.Join(", ",
                columns.Select(c =>
                    $"{c.Name} {c.TypeName}"));

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                $"CREATE TABLE {tableName} ({columnDefs});";

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task AddMissingColumnsAsync(
            SqliteConnection connection,
            string tableName,
            List<ColumnInfo> existing,
            List<ColumnInfo> desired)
        {
            HashSet<string> existingSet = existing
                .Select(c => c.Name.ToUpper())
                .ToHashSet();

            foreach (ColumnInfo col in desired)
            {
                if (existingSet.Contains(col.Name.ToUpper()))
                    continue;

                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText =
                    $"ALTER TABLE {tableName} ADD COLUMN {col.Name} {col.TypeName};";

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
