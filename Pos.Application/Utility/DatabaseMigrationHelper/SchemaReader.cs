using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pos.Application.DTOs.DatabaseMigrationDtos;

namespace Pos.Application.Utility.DatabaseMigrationHelper
{

    public class SchemaReader
    {
        // -----------------------------
        // DESIRED SCHEMA (FROM DB CONTEXT)
        // -----------------------------
        public Dictionary<string, List<ColumnInfo>> ReadDesiredSchema(DbContext context)
        {
            Dictionary<string, List<ColumnInfo>> schema = new(StringComparer.OrdinalIgnoreCase);

            foreach (IEntityType entityType in context.Model.GetEntityTypes())
            {
                string? tableName = entityType.GetTableName();

                if (string.IsNullOrWhiteSpace(tableName))
                    continue;

                List<ColumnInfo> columns = entityType.GetProperties()
                    .Select(p => new ColumnInfo
                    {
                        Name = p.GetColumnName(),
                        TypeName = p.GetColumnType() ?? "TEXT",
                        IsNullable = p.IsNullable,
                        IsPrimaryKey = p.IsPrimaryKey()
                    })
                    .ToList();

                schema[tableName] = columns;
            }

            return schema;
        }

        // -----------------------------
        // EXISTING SCHEMA (FROM SQLITE)
        // -----------------------------
        public async Task<Dictionary<string, List<ColumnInfo>>> ReadExistingSchemaAsync(SqliteConnection connection)
        {
            Dictionary<string, List<ColumnInfo>> schema = new(StringComparer.OrdinalIgnoreCase);

            // 1. tables
            SqliteCommand tablesCmd = connection.CreateCommand();
            tablesCmd.CommandText =
                @"SELECT name FROM sqlite_master 
              WHERE type='table' AND name NOT LIKE 'sqlite_%';";

            List<string> tables = new();

            using (SqliteDataReader reader = await tablesCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    tables.Add(reader.GetString(0));
            }

            // 2. columns per table
            foreach (string table in tables)
            {
                List<ColumnInfo> columns = new();

                SqliteCommand pragma = connection.CreateCommand();
                pragma.CommandText = $"PRAGMA table_info([{table}]);";

                using SqliteDataReader reader = await pragma.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnInfo
                    {
                        Name = reader.GetString(1),
                        TypeName = reader.GetString(2),
                        IsNullable = reader.GetInt32(3) == 0,
                        IsPrimaryKey = reader.GetInt32(5) == 1
                    });
                }

                schema[table] = columns;
            }

            return schema;
        }
    }
}

