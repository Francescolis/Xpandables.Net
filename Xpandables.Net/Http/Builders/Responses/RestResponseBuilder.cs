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
public sealed class RestResponseBuilder : IRestResponseBuilder
{
    /// <inheritdoc/>
    public async Task<RestResponse> BuildAsync(
        RestResponseContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        HttpResponseMessage response = context.Message;
        JsonSerializerOptions options = context.SerializerOptions;

        try
        {
            if (!response.IsSuccessStatusCode)
            {
                // Handle unsuccessful response
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

            // Content exists
            string contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            // stream content
            if (contentType.Contains("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                Stream stream = await response.Content
                    .ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version,
                    Result = stream
                };
            }

            // json content
            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                string json = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(json))
                {
                    return new RestResponse
                    {
                        StatusCode = response.StatusCode,
                        ReasonPhrase = response.ReasonPhrase,
                        Headers = response.Headers.ToElementCollection(),
                        Version = response.Version
                    };
                }

                object? result = JsonSerializer.Deserialize<object>(json, options);

                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version,
                    Result = result
                };
            }

            // other content types
            string content = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Result = content
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
        finally
        {
            response.Dispose();
        }
    }
}
