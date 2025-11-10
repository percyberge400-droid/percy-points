using System.Reflection;
using AutoMapper;

namespace Pos.Application.Utility
{
    public class DictionaryToDtoConverter<TDestination>
        : ITypeConverter<Dictionary<string, object>, TDestination>
        where TDestination : new()
    {
        public TDestination Convert(
            Dictionary<string, object> source,
            TDestination destination,
            ResolutionContext context)
        {
            destination ??= new TDestination();

            foreach (var prop in typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (source.TryGetValue(prop.Name, out var val) && val != null)
                {
                    // Explicitly reference System.Convert to avoid conflict
                    prop.SetValue(destination,
                        System.Convert.ChangeType(val, prop.PropertyType));
                }
            }

            return destination;
        }
    }
}
