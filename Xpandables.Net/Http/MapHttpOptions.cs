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
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xpandables.Net.Http.RequestBuilders;
using Xpandables.Net.Http.ResponseBuilders;
using Xpandables.Net.Optionals;
using Xpandables.Net.Text;

namespace Xpandables.Net.Http;
/// <summary>
/// Represents the options for to manage <see cref="IHttpRequestFactory"/>
/// and <see cref="IHttpResponseFactory"/> and its associated services.
/// </summary>
public sealed record MapHttpOptions
{
    /// <summary>
    /// Gets the list of user-defined response builders that were registered.
    /// </summary>
    public Collection<IHttpResponseBuilder> ResponseBuilders { get; } = [];

    /// <summary>
    /// Gets the list of user-defined request builders that were registered.
    /// </summary>
    public Collection<IHttpRequestBuilder> RequestBuilders { get; } = [];

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
    public IHttpRequestBuilder GetRequestBuilder(Type requestType)
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
    public IHttpRequestBuilder GetRequestBuilder<TRequest>()
       where TRequest : IHttpRequest
       => GetRequestBuilder(typeof(TRequest));

    /// <summary>  
    /// Gets the request builders for the specified request type.  
    /// </summary>  
    /// <param name="requestType">The type of the request.</param>  
    /// <returns>The request builders for the specified request type.</returns>  
    public IEnumerable<IHttpRequestBuilder> GetAllRequestBuilders(
        Type requestType)
       => RequestBuilders
           .Where(x => x.CanBuild(requestType));

    /// <summary>  
    /// Gets the request builders for the specified request type.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <returns>The request builders for the specified request type.</returns>  
    public IEnumerable<IHttpRequestBuilder> GetAllRequestBuilders<TRequest>()
        where TRequest : IHttpRequest
        => GetAllRequestBuilders(typeof(TRequest));

    /// <summary>  
    /// Gets the response builder for the specified response type and status code.  
    /// </summary>  
    /// <param name="responseType">The type of the response.</param>  
    /// <param name="statusCode">The HTTP status code.</param>  
    /// <returns>The response builder for the specified response type and status code.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no response 
    /// builder is found for the specified response type and status code.</exception>  
    public IHttpResponseBuilder GetResponseBuilder(
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
    public IHttpResponseBuilder GetResponseBuilder<TResponse>(
        HttpStatusCode statusCode)
        where TResponse : HttpResponse
        => GetResponseBuilder(typeof(TResponse), statusCode);

    /// <summary>  
    /// Gets the map request for the specified request.  
    /// </summary>  
    /// <param name="request">The HTTP client request.</param>  
    /// <returns>The map request attribute for the specified request.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when the request is 
    /// not decorated with <see cref="MapHttpAttribute"/> or 
    /// does not implement <see cref="IMapHttpBuilder"/>.</exception>  
    public MapHttpAttribute GetMapHttp(
       IHttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request is IMapHttpBuilder builder
            ? builder.Build(this)
            : request
                .GetType()
                .GetCustomAttribute<MapHttpAttribute>(true)
                ?? throw new InvalidOperationException(
                    $"Request must be decorated with one of the {nameof(MapHttpAttribute)} " +
                    $"or implement {nameof(IMapHttpBuilder)}");
    }

    /// <summary>
    /// Configures the default HTTP client options.
    /// </summary>
    /// <param name="options">The HTTP client options.</param>
    public static void Default(MapHttpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // response builders
        options.ResponseBuilders.Add(new HttpResponseFailureStreamBuilder());
        options.ResponseBuilders.Add(new HttpResponseFailureBuilder());
        options.ResponseBuilders.Add(new HttpResponseFailureResultBuilder());
        options.ResponseBuilders.Add(new HttpResponseSuccessStreamBuilder());
        options.ResponseBuilders.Add(new HttpResponseSuccessBuilder());
        options.ResponseBuilders.Add(new HttpResponseSuccessResultBuilder());

        // request builders
        options.RequestBuilders.Add(new HttpRequestPathStringBuilder());
        options.RequestBuilders.Add(new HttpRequestQueryStringBuilder());
        options.RequestBuilders.Add(new HttpRequestCookieBuilder());
        options.RequestBuilders.Add(new HttpRequestHeaderBuilder());
        options.RequestBuilders.Add(new HttpRequestBasicAuthenticationBuilder());
        options.RequestBuilders.Add(new HttpRequestByteArrayBuilder());
        options.RequestBuilders.Add(new HttpRequestFormUrlEncodedBuilder());
        options.RequestBuilders.Add(new HttpRequestMultipartBuilder());
        options.RequestBuilders.Add(new HttpRequestStreamBuilder());
        options.RequestBuilders.Add(new HttpRequestPatchBuilder());
        options.RequestBuilders.Add(new HttpRequestStringBuilder());

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
    /// Gets the default HTTP request options.  
    /// </summary>  
    public static MapHttpOptions DefaultRequestOptions
    {
        get
        {
            MapHttpOptions options = new();
            Default(options);
            return options;
        }
    }
}
