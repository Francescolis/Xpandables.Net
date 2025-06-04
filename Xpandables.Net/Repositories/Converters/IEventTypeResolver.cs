using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.Repositories.Converters;

/// <summary>
/// Defines a contract for resolving an event type name to its JsonTypeInfo.
/// </summary>
public interface IEventTypeResolver
{
    /// <summary>
    /// Gets the JsonTypeInfo for the specified event type name.
    /// </summary>
    /// <param name="typeName">The name of the event type (e.g., full name or short name).</param>
    /// <returns>The JsonTypeInfo if found; otherwise, null.</returns>
    JsonTypeInfo? GetJsonTypeInfo(string typeName);
}
