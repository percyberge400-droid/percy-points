using System.Data;
using System.Reflection;

namespace Pos.Application.Utility
{
    public static class SharedResources
    {
        /// <summary>
        /// Converts a list of objects into a DataTable.
        /// </summary>
        public static DataTable ToDataTable<T>(IList<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            // Get all public instance properties of T
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Add columns with proper type (handles nullable types too)
            foreach (var prop in properties)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, type);
            }

            // Add rows
            foreach (var item in items)
            {
                var values = properties.Select(p => p.GetValue(item, null) ?? DBNull.Value).ToArray();
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
}
