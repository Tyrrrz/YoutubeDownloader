using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JsonExtensions.Reading;

namespace YoutubeExplode.Utils.Extensions;

internal static class JsonExtensions
{
    extension(JsonElement element)
    {
        public IEnumerable<JsonElement> EnumerateDescendantProperties(string propertyName)
        {
            // Check if this property exists on the current object
            var property = element.GetPropertyOrNull(propertyName);
            if (property is not null)
                yield return property.Value;

            // Recursively check on all array children (if current element is an array)
            var deepArrayDescendants = element
                .EnumerateArrayOrEmpty()
                .SelectMany(j => j.EnumerateDescendantProperties(propertyName));

            foreach (var deepDescendant in deepArrayDescendants)
                yield return deepDescendant;

            // Recursively check on all object children (if current element is an object)
            var deepObjectDescendants = element
                .EnumerateObjectOrEmpty()
                .SelectMany(j => j.Value.EnumerateDescendantProperties(propertyName));

            foreach (var deepDescendant in deepObjectDescendants)
                yield return deepDescendant;
        }
    }
}
