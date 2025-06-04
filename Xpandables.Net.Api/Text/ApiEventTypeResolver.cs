using System.Text.Json.Serialization.Metadata;
using Xpandables.Net.Repositories.Converters; // For IEventTypeResolver

namespace Xpandables.Net.Api.Text;

/// <summary>
/// Implements IEventTypeResolver using the ApiJsonSerializerContext.
/// </summary>
public class ApiEventTypeResolver : IEventTypeResolver
{
    // No constructor needed as ApiJsonSerializerContext.EventTypeInfoMap is static and public.

    /// <inheritdoc/>
    public JsonTypeInfo? GetJsonTypeInfo(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        // Use the static map from ApiJsonSerializerContext
        if (ApiJsonSerializerContext.EventTypeInfoMap.TryGetValue(typeName, out JsonTypeInfo? typeInfo))
        {
            return typeInfo;
        }

        // Consider logging if a type is not found, depending on strictness requirements.
        // For now, returning null indicates the type is not resolvable by this resolver.
        return null;
    }
}
