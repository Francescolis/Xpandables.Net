
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
using System.Net;

namespace Xpandables.Net.Http.ResponseBuilders;
/// <summary>
/// A builder for creating successful HTTP client responses.
/// </summary>
public sealed class HttpClientResponseSuccessBuilder : IHttpClientResponseBuilder
{
    /// <inheritdoc/>
    public Type Type => typeof(HttpClientResponse);

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        targetType == Type && (int)statusCode is >= 200 and <= 299;

    /// <inheritdoc/>
    public Task<TResponse> BuildAsync<TResponse>(
        HttpClientResponseContext context,
        CancellationToken cancellationToken = default)
        where TResponse : HttpClientResponse
    {
        if (!CanBuild(typeof(TResponse), context.Message.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and success status code.",
                new NotSupportedException("Unsupported response type"));
        }

        if (context.Message
            .Content
            .Headers
            .ContentDisposition is null)
        {
            HttpClientResponse response = new()
            {
                StatusCode = context.Message.StatusCode,
                Headers = context.Message.ToNameValueCollection(),
                Version = context.Message.Version,
                ReasonPhrase = context.Message.ReasonPhrase
            };

            return Task.FromResult((TResponse)response);
        }

        string fileName = context.Message
            .Content
            .Headers
            .ContentDisposition
            .FileName!
            .Trim('"');

        Uri requestUri = context
            .Message
            .RequestMessage!
            .RequestUri!;

        string baseUrl = requestUri.GetLeftPart(UriPartial.Authority);

        string fileUrl = $"{baseUrl}/{Uri.EscapeDataString(fileName)}";

        System.Collections.Specialized.NameValueCollection headers
            = context.Message.ToNameValueCollection();

        headers.Add("Location", fileUrl);

        HttpClientResponse responseFile = new()
        {
            StatusCode = context.Message.StatusCode,
            Headers = headers,
            Version = context.Message.Version,
            ReasonPhrase = context.Message.ReasonPhrase
        };

        return Task.FromResult((TResponse)responseFile);
    }
}
