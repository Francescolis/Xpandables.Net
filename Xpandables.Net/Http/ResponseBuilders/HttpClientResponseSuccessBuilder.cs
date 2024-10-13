
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
    /// <summary>
    /// Gets the type of the response.
    /// </summary>
    public Type Type => typeof(HttpClientResponse);

    /// <summary>
    /// Determines whether this builder can build a response for the specified 
    /// target type and status code.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns><c>true</c> if this builder can build the response; 
    /// otherwise, <c>false</c>.</returns>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        targetType == Type && (int)statusCode is >= 200 and <= 299;

    /// <summary>
    /// Builds the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <returns>A delegate that builds the response.</returns>
    public ResponseBuilderDelegate<TResponse> Builder<TResponse>()
        where TResponse : HttpClientResponse =>
        (context, cancellationToken) =>
        {
            if (context.ResponseMessage
                .Content
                .Headers
                .ContentDisposition is null)
            {
                HttpClientResponse response = new()
                {
                    StatusCode = context.ResponseMessage.StatusCode,
                    Headers = context.ResponseMessage.ToNameValueCollection(),
                    Version = context.ResponseMessage.Version,
                    ReasonPhrase = context.ResponseMessage.ReasonPhrase
                };

                return Task.FromResult((TResponse)response);
            }

            string fileName = context.ResponseMessage
                .Content
                .Headers
                .ContentDisposition
                .FileName!
                .Trim('"');

            Uri requestUri = context
                .ResponseMessage
                .RequestMessage!
                .RequestUri!;

            string baseUrl = requestUri.GetLeftPart(UriPartial.Authority);

            string fileUrl = $"{baseUrl}/{Uri.EscapeDataString(fileName)}";

            System.Collections.Specialized.NameValueCollection headers
                = context.ResponseMessage.ToNameValueCollection();

            headers.Add("Location", fileUrl);

            HttpClientResponse responseFile = new()
            {
                StatusCode = context.ResponseMessage.StatusCode,
                Headers = headers,
                Version = context.ResponseMessage.Version,
                ReasonPhrase = context.ResponseMessage.ReasonPhrase
            };

            return Task.FromResult((TResponse)responseFile);
        };
}
