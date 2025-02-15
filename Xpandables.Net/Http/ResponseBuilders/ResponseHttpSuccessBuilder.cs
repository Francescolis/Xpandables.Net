
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

using Xpandables.Net.Executions;

namespace Xpandables.Net.Http.ResponseBuilders;
/// <summary>
/// A builder for creating successful HTTP client responses.
/// </summary>
public sealed class ResponseHttpSuccessBuilder : IResponseHttpBuilder
{
    /// <inheritdoc/>
    public Type Type => typeof(ResponseHttp);

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        targetType == Type
        && statusCode.IsSuccessStatusCode();

    /// <inheritdoc/>
    public Task<TResponse> BuildAsync<TResponse>(
        ResponseContext context,
        CancellationToken cancellationToken = default)
        where TResponse : ResponseHttp
    {
        ArgumentNullException.ThrowIfNull(context);

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
            ResponseHttp response = new()
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

        ResponseHttp responseFile = new()
        {
            StatusCode = context.Message.StatusCode,
            Headers = headers,
            Version = context.Message.Version,
            ReasonPhrase = context.Message.ReasonPhrase
        };

        return Task.FromResult((TResponse)responseFile);
    }
}
