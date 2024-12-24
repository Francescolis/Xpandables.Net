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
using System.Text.Json.Serialization;

using Xpandables.Net.Http.Interfaces;
using Xpandables.Net.Http.RequestBuilders;
using Xpandables.Net.Http.ResponseBuilders;
using Xpandables.Net.Optionals;
using Xpandables.Net.Text;

namespace Xpandables.Net.Http;
/// <summary>
/// Represents the options for to manage <see cref="IHttpClientMessageFactory"/>
/// factory and its associated services.
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
    /// Gets the resolver function for resolving types.
    /// </summary>
    public Func<Type, object?>? Resolver { get; internal set; }

    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> to be used.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; }
        = DefaultSerializerOptions.Defaults;

    /// <summary>  
    /// Gets the request builder for the specified request type.  
    /// </summary>  
    /// <param name="requestType">The type of the request.</param>  
    /// <returns>The request builder for the specified request type.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no request 
    /// builder is found for the specified request type.</exception>  
    public IHttpClientRequestBuilder PeekRequestBuilder(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);

        return RequestBuilders
            .FirstOrDefault(x => x.CanBuild(requestType))
            ?? throw new InvalidOperationException(
                $"No request builder found for the request type '{requestType.Name}'.");
    }

    /// <summary>  
    /// Gets the request builder for the specified request type.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <returns>The request builder for the specified request type.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no request  
    /// builder is found for the specified request type.</exception>  
    public IHttpClientRequestBuilder PeekRequestBuilder<TRequest>()
       where TRequest : IHttpClientRequest
       => PeekRequestBuilder(typeof(TRequest));

    /// <summary>  
    /// Gets the request builders for the specified request type.  
    /// </summary>  
    /// <param name="requestType">The type of the request.</param>  
    /// <returns>The request builders for the specified request type.</returns>  
    public IEnumerable<IHttpClientRequestBuilder> PeekRequestBuilders(
        Type requestType)
       => RequestBuilders
           .Where(x => x.CanBuild(requestType));

    /// <summary>  
    /// Gets the request builders for the specified request type.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <returns>The request builders for the specified request type.</returns>  
    public IEnumerable<IHttpClientRequestBuilder> PeekRequestBuilders<TRequest>()
        where TRequest : IHttpClientRequest
        => PeekRequestBuilders(typeof(TRequest));

    /// <summary>  
    /// Gets the response builder for the specified response type and status code.  
    /// </summary>  
    /// <param name="responseType">The type of the response.</param>  
    /// <param name="statusCode">The HTTP status code.</param>  
    /// <returns>The response builder for the specified response type and status code.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no response 
    /// builder is found for the specified response type and status code.</exception>  
    public IHttpClientResponseBuilder PeekResponseBuilder(
        Type responseType, HttpStatusCode statusCode)
    {
        ArgumentNullException.ThrowIfNull(responseType);

        return ResponseBuilders
            .FirstOrDefault(x => x.CanBuild(responseType, statusCode))
            ?? throw new InvalidOperationException(
                $"No response builder found for the response type '{responseType.Name}'.");
    }

    /// <summary>  
    /// Gets the response builder for the specified response type and status code.  
    /// </summary>  
    /// <typeparam name="TResponse">The type of the response.</typeparam>  
    /// <param name="statusCode">The HTTP status code.</param>  
    /// <returns>The response builder for the specified response type and status code.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no response 
    /// builder is found for the specified response type and status code.</exception>  
    public IHttpClientResponseBuilder PeekResponseBuilder<TResponse>(
        HttpStatusCode statusCode)
        where TResponse : HttpClientResponse
        => PeekResponseBuilder(typeof(TResponse), statusCode);

    /// <summary>  
    /// Gets the request options for the specified request.  
    /// </summary>  
    /// <param name="request">The HTTP client request.</param>  
    /// <returns>The request options attribute for the specified request.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when the request is 
    /// not decorated with <see cref="HttpClientAttribute"/> or 
    /// does not implement <see cref="IHttpClientAttributeBuilder"/>.</exception>  
    public HttpClientAttribute GetRequestOptions(
       IHttpClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request is IHttpClientAttributeBuilder builder
            ? builder.Build(this)
            : request
                .GetType()
                .GetCustomAttribute<HttpClientAttribute>(true)
                ?? throw new InvalidOperationException(
                    $"Request must be decorated with {nameof(HttpClientAttribute)} " +
                    $"or implement {nameof(IHttpClientAttributeBuilder)}");
    }

    /// <summary>
    /// Configures the default HTTP client options.
    /// </summary>
    /// <param name="options">The HTTP client options.</param>
    public static void Default(HttpClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _ = options.ResponseBuilders
            .Add(new HttpClientResponseFailureAsyncResultBuilder());
        _ = options.ResponseBuilders
            .Add(new HttpClientResponseFailureBuilder());
        _ = options.ResponseBuilders
            .Add(new HttpClientResponseFailureResultBuilder());
        _ = options.ResponseBuilders
            .Add(new HttpClientResponseSuccessAsyncResultBuilder());
        _ = options.ResponseBuilders
            .Add(new HttpClientResponseSuccessBuilder());
        _ = options.ResponseBuilders
            .Add(new HttpClientResponseSuccessResultBuilder());

        _ = options.RequestBuilders
            .Add(new HttpClientBasicAuthRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientByteArrayRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientCompleteRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientCookieRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientFormUrlEncodedRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientHeaderRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientMultipartRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientPatchRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientPathStringRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientQueryStringRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientStartRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientStreamRequestBuilder());
        _ = options.RequestBuilders
            .Add(new HttpClientStringRequestBuilder());

        options.SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }

    /// <summary>  
    /// Gets the default HTTP client options.  
    /// </summary>  
    public static HttpClientOptions DefaultHttpClientOptions
    {
        get
        {
            HttpClientOptions options = new();
            Default(options);
            return options;
        }
    }
}
