using System;
using System.Text.Json.Serialization.Metadata;
using Xpandables.Net.Executions.Rests.Responses; // For IRestResponseTypeResolver

namespace Xpandables.Net.Api.Text;

/// <summary>
/// Implements IRestResponseTypeResolver using the ApiJsonSerializerContext.
/// </summary>
public class ApiRestResponseTypeResolver : IRestResponseTypeResolver
{
    private readonly ApiJsonSerializerContext _jsonContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRestResponseTypeResolver"/> class.
    /// </summary>
    /// <param name="jsonContext">The API JSON serializer context.</param>
    public ApiRestResponseTypeResolver(ApiJsonSerializerContext jsonContext)
    {
        ArgumentNullException.ThrowIfNull(jsonContext);
        _jsonContext = jsonContext;
    }

    /// <inheritdoc/>
    public JsonTypeInfo? GetJsonTypeInfo(Type resultType)
    {
        ArgumentNullException.ThrowIfNull(resultType);

        try
        {
            // This relies on ApiJsonSerializerContext having [JsonSerializable] for all expected 'resultType's.
            return _jsonContext.GetTypeInfo(resultType);
        }
        catch (InvalidOperationException) // GetTypeInfo throws if type not found
        {
            // Type not found in the context
            return null;
        }
    }
}
