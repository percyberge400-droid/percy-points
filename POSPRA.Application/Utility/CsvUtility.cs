using System.Reflection;
using System.Text;

namespace POSPRA.Application.Utility
{
    public static class CsvUtility
    {
        /// <summary>
        /// Converts a sequence of objects to a CSV string using public properties.
        /// </summary>
        public static string ToCsv<T>(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var sb = new StringBuilder();
            var props = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToArray();

            // Header row
            sb.AppendLine(string.Join(",", props.Select(p => p.Name)));

            // Data rows
            foreach (var item in items)
            {
                var values = props.Select(p =>
                {
                    var val = p.GetValue(item);
                    if (val == null) return string.Empty;

                    var str = val.ToString();

                    // Escape quotes & commas
                    if (str.Contains("\"")) str = str.Replace("\"", "\"\"");
                    if (str.Contains(",") || str.Contains("\n") || str.Contains("\r"))
                        str = $"\"{str}\"";

                    return str;
                });

                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }
    }
}