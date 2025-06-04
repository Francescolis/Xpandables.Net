using System.Text.Json.Serialization.Metadata; // Added for JsonTypeInfo
/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/

using System.Text.Json;

namespace Xpandables.Net.Executions.Rests.Responses;

/// <summary>
/// Composes a result RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
/// <typeparam name="TRestRequest"> The type of the REST request.</typeparam> 
using Xpandables.Net.Executions.Rests.Responses; // Required for IRestResponseTypeResolver

public sealed class RestResponseResultComposer<TRestRequest> : IRestResponseComposer<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    private readonly IRestResponseTypeResolver _responseTypeResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestResponseResultComposer{TRestRequest}"/> class.
    /// </summary>
    /// <param name="responseTypeResolver">The resolver for REST response types.</param>
    /// <exception cref="ArgumentNullException">Thrown if responseTypeResolver is null.</exception>
    public RestResponseResultComposer(IRestResponseTypeResolver responseTypeResolver)
    {
        ArgumentNullException.ThrowIfNull(responseTypeResolver);
        _responseTypeResolver = responseTypeResolver;
    }

    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext<TRestRequest> context) =>
            context.Message.IsSuccessStatusCode
            && context.Request.ResultType is not null
            && !context.Request.IsRequestStream;

    /// <inheritdoc/>
    public async ValueTask<RestResponse> ComposeAsync(
        RestResponseContext<TRestRequest> context, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = context.Message;
        JsonSerializerOptions options = context.SerializerOptions;
        TRestRequest request = context.Request;

        if (!CanCompose(context))
            throw new InvalidOperationException(
                $"{nameof(ComposeAsync)}: The response is not a success. " +
                $"Status code: {response.StatusCode} ({response.ReasonPhrase}).");

        try
        {
            Type resultType = request.ResultType!;

            string stringContent = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(stringContent))
            {
                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version
                };
            }

            JsonTypeInfo? resultTypeInfo = _responseTypeResolver.GetJsonTypeInfo(resultType);
            if (resultTypeInfo == null)
            {
                // Log this failure or handle more gracefully if some types are intentionally not AOT-ready.
                // Forcing AOT safety means all resolvable types must be in a context.
                throw new InvalidOperationException(
                    $"Could not resolve JsonTypeInfo for result type {resultType.FullName}. " +
                    "Ensure this type is included in a JsonSerializerContext and the IRestResponseTypeResolver is correctly configured.");
            }

            // The 'options' parameter might still be useful if it contains other configurations
            // not covered by JsonTypeInfo (e.g., custom converters not part of the context).
            // However, JsonSerializer.Deserialize(string, JsonTypeInfo) is the primary AOT-safe overload.
            // If 'options' are needed, one would typically get JsonTypeInfo via options.GetTypeInfo(type),
            // but here we explicitly use a resolver that should be context-aware.
            // object? typedResult = JsonSerializer.Deserialize(stringContent, resultTypeInfo); // No direct string overload with JsonTypeInfo only

            // Convert string to JsonDocument first, then deserialize with JsonTypeInfo
            using JsonDocument jsonDocument = JsonDocument.Parse(stringContent);
            object? typedResult = JsonSerializer.Deserialize(jsonDocument.RootElement, resultTypeInfo);


            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Result = typedResult
            };
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Exception = exception
            };
        }
    }
}
