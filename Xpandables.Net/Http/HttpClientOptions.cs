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
using System.Reflection;
using System.Text.Json;

namespace Xpandables.Net.Http;
/// <summary>
/// Represents the options for configuring an HTTP client.
/// </summary>
public sealed record HttpClientOptions
{
    /// <summary>
    /// Gets the list of user-defined response builders that were registered.
    /// </summary>
    public HashSet<IHttpClientResponseBuilder> ResponseBuilders { get; }
        = [];

    /// <summary>
    /// Gets the list of user-defined request builders that were registered.
    /// </summary>
    public HashSet<IHttpClientRequestBuilder> RequestBuilders { get; }
        = [];

    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> to be used.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; }
        = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = true,
        };

    /// <summary>  
    /// Gets the request builder for the specified request type.  
    /// </summary>  
    /// <param name="requestType">The type of the request.</param>  
    /// <returns>The request builder for the specified request type.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no request 
    /// builder is found for the specified request type.</exception>  
    public IHttpClientRequestBuilder GetRequestBuilder(Type requestType)
       => RequestBuilders
           .FirstOrDefault(x => x.CanBuild(requestType))
           ?? throw new InvalidOperationException(
               $"No request builder found for the request type '{requestType.Name}'.");

    /// <summary>  
    /// Gets the request builder for the specified request type.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <returns>The request builder for the specified request type.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no request  
    /// builder is found for the specified request type.</exception>  
    public IHttpClientRequestBuilder GetRequestBuilder<TRequest>()
       where TRequest : IHttpClientRequest
       => GetRequestBuilder(typeof(TRequest));

    /// <summary>  
    /// Gets the request builders for the specified request type.  
    /// </summary>  
    /// <param name="requestType">The type of the request.</param>  
    /// <returns>The request builders for the specified request type.</returns>  
    public IEnumerable<IHttpClientRequestBuilder> GetRequestBuilders(
        Type requestType)
       => RequestBuilders
           .Where(x => x.CanBuild(requestType));

    /// <summary>  
    /// Gets the request builders for the specified request type.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <returns>The request builders for the specified request type.</returns>  
    public IEnumerable<IHttpClientRequestBuilder> GetRequestBuilders<TRequest>()
        where TRequest : IHttpClientRequest
        => GetRequestBuilders(typeof(TRequest));

    /// <summary>  
    /// Gets the response builder for the specified response type and status code.  
    /// </summary>  
    /// <param name="responseType">The type of the response.</param>  
    /// <param name="statusCode">The HTTP status code.</param>  
    /// <returns>The response builder for the specified response type and status code.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no response 
    /// builder is found for the specified response type and status code.</exception>  
    public IHttpClientResponseBuilder GetResponseBuilder(
        Type responseType, HttpStatusCode statusCode)
        => ResponseBuilders
            .FirstOrDefault(x => x.CanBuild(responseType, statusCode))
            ?? throw new InvalidOperationException(
                $"No response builder found for the response type '{responseType.Name}'.");

    /// <summary>  
    /// Gets the response builder for the specified response type and status code.  
    /// </summary>  
    /// <typeparam name="TResponse">The type of the response.</typeparam>  
    /// <param name="statusCode">The HTTP status code.</param>  
    /// <returns>The response builder for the specified response type and status code.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no response 
    /// builder is found for the specified response type and status code.</exception>  
    public IHttpClientResponseBuilder GetResponseBuilder<TResponse>(
        HttpStatusCode statusCode)
        where TResponse : HttpClientResponse
        => GetResponseBuilder(typeof(TResponse), statusCode);

    /// <summary>  
    /// Gets the request options for the specified request.  
    /// </summary>  
    /// <param name="request">The HTTP client request.</param>  
    /// <returns>The request options attribute for the specified request.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when the request is not decorated with <see cref="HttpClientRequestOptionsAttribute"/> or does not implement <see cref="IHttpClientRequestOptionsBuilder"/>.</exception>  
    public HttpClientRequestOptionsAttribute GetRequestOptions(
       IHttpClientRequest request)
       => request
           .GetType()
           .GetCustomAttribute<HttpClientRequestOptionsAttribute>(true)
           ?? throw new InvalidOperationException(
               $"Request must be decorated with {nameof(HttpClientRequestOptionsAttribute)} " +
               $"or implement {nameof(IHttpClientRequestOptionsBuilder)}");
}
