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

using Xpandables.Net.Executions;

namespace Xpandables.Net.Http.Builders.Responses;

/// <summary>
/// Builds a RestResponse asynchronously using the provided RestResponseContext. Supports cancellation through a token.
/// </summary>
/// <typeparam name="TRestRequest"> The type of the REST request.</typeparam> 
public sealed class RestResponseBuilder<TRestRequest> : IRestResponseBuilder<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    /// <inheritdoc/>
    public async Task<RestResponse> BuildAsync(
        RestResponseContext<TRestRequest> context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        HttpResponseMessage response = context.Message;
        JsonSerializerOptions options = context.SerializerOptions;

        try
        {
            // Handle unsuccessful response
            if (!response.IsSuccessStatusCode)
            {
                string? errorContent = default;
                if (response.Content is not null)
                {
                    errorContent = await response.Content
                        .ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                errorContent = $"Response status code does not indicate success: " +
                    $"{(int)response.StatusCode} ({response.ReasonPhrase}). {errorContent}";

                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version,
                    Exception = response.StatusCode.GetAppropriateException(errorContent)
                };
            }

            // No content case
            if (response.Content is null || response.Content.Headers.ContentLength == 0)
            {
                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version
                };
            }

            // Determine request type
            bool isRestRequestResult = typeof(TRestRequest).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRestRequest<>));
            bool isRestRequestStreamResult = typeof(TRestRequest).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRestRequestStream<>));

            if (isRestRequestResult)
            {
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

                // Deserialize to the specific type requested
                Type resultType = typeof(TRestRequest)
                    .GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRestRequest<>))
                    .GetGenericArguments()[0];
                object? typedResult = JsonSerializer.Deserialize(stringContent, resultType, options);

                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version,
                    Result = typedResult
                };
            }

            if (isRestRequestStreamResult)
            {
                Stream stream = await response.Content
                    .ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (stream is null)
                {
                    return new RestResponse
                    {
                        StatusCode = response.StatusCode,
                        ReasonPhrase = response.ReasonPhrase,
                        Headers = response.Headers.ToElementCollection(),
                        Version = response.Version
                    };
                }

                // Deserialize to the specific type requested
                Type resultType = typeof(TRestRequest)
                    .GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRestRequestStream<>))
                    .GetGenericArguments()[0];

                object typedResult = stream.DeserializeAsyncEnumerableAsync(resultType, options, cancellationToken);

                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version,
                    Result = typedResult
                };
            }

            string generalContent = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Result = generalContent
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
