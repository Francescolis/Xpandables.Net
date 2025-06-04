using System.Text.Json.Serialization.Metadata;
using Xpandables.Net.Repositories.Converters; // For IEventTypeResolver

namespace Xpandables.Net.Api.Text;

/// <summary>
/// Implements IEventTypeResolver using the ApiJsonSerializerContext.
/// </summary>
public class ApiEventTypeResolver : IEventTypeResolver
{
    // No constructor needed if ApiJsonSerializerContext.EventTypeInfoMap is static and public.
    // If it were instance-based, an ApiJsonSerializerContext would be injected here.

    /// <inheritdoc/>
    public JsonTypeInfo? GetJsonTypeInfo(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        // Use the static map from ApiJsonSerializerContext
        // This map should be populated with both FullName and Name keys if they differ.
        if (ApiJsonSerializerContext.EventTypeInfoMap.TryGetValue(typeName, out JsonTypeInfo? typeInfo))
        {
            return typeInfo;
        }

        // Optional: Could add more sophisticated lookup logic here if needed,
        // e.g., trying to match short names if full names fail, or vice-versa,
        // though the current TryAddEventTypeInfo in ApiJsonSerializerContext already adds both.

        // Consider logging if a type is not found, depending on strictness requirements.
        // For now, returning null indicates the type is not resolvable by this resolver.
        return null;
    }
}
