namespace Pos.Application.DTOs.DatabaseMigrationDtos
{
    public class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}
