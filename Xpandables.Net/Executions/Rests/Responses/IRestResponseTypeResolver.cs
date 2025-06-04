using System;
using System.Text.Json.Serialization.Metadata;

namespace Xpandables.Net.Executions.Rests.Responses;

/// <summary>
/// Defines a contract for resolving a result type to its JsonTypeInfo for REST responses.
/// </summary>
public interface IRestResponseTypeResolver
{
    /// <summary>
    /// Gets the JsonTypeInfo for the specified result type.
    /// </summary>
    /// <param name="resultType">The type of the result to deserialize.</param>
    /// <returns>The JsonTypeInfo if found; otherwise, null.</returns>
    JsonTypeInfo? GetJsonTypeInfo(Type resultType);
}
