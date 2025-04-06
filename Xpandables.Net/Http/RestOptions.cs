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

using Xpandables.Net.Http.Builders;
using Xpandables.Net.Http.Builders.Requests;
using Xpandables.Net.Http.Builders.Responses;
using Xpandables.Net.Optionals;
using Xpandables.Net.Text;

namespace Xpandables.Net.Http;
/// <summary>
/// Represents the options for to manage <see cref="IRestRequestFactory"/>
/// and <see cref="IRestResponseFactory"/> and its associated services.
/// </summary>
public sealed record RestOptions
{
    /// <summary>
    /// Gets the list of user-defined response builders that were registered.
    /// </summary>
    public Collection<IRestResponseBuilder> ResponseBuilders { get; } = [];

    /// <summary>
    /// Gets the list of user-defined request builders that were registered.
    /// </summary>
    public Collection<IRestRequestBuilder> RequestBuilders { get; } = [];

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
    public IRestRequestBuilder GetRequestBuilder(Type requestType)
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
    public IRestRequestBuilder GetRequestBuilder<TRequest>()
       where TRequest : IRestRequest
       => GetRequestBuilder(typeof(TRequest));

    /// <summary>  
    /// Gets the request builders for the specified request type.  
    /// </summary>  
    /// <param name="requestType">The type of the request.</param>  
    /// <returns>The request builders for the specified request type.</returns>  
    public IEnumerable<IRestRequestBuilder> GetAllRequestBuilders(Type requestType)
       => RequestBuilders
           .Where(x => x.CanBuild(requestType));

    /// <summary>  
    /// Gets the request builders for the specified request type.  
    /// </summary>  
    /// <typeparam name="TRequest">The type of the request.</typeparam>  
    /// <returns>The request builders for the specified request type.</returns>  
    public IEnumerable<IRestRequestBuilder> GetAllRequestBuilders<TRequest>()
        where TRequest : IRestRequest
        => GetAllRequestBuilders(typeof(TRequest));

    /// <summary>  
    /// Gets the response builder for the specified response type and status code.  
    /// </summary>  
    /// <param name="responseType">The type of the response.</param>  
    /// <param name="statusCode">The HTTP status code.</param>  
    /// <returns>The response builder for the specified response type and status code.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when no response 
    /// builder is found for the specified response type and status code.</exception>  
    public IRestResponseBuilder GetResponseBuilder(Type responseType, HttpStatusCode statusCode)
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
    public IRestResponseBuilder GetResponseBuilder<TResponse>(HttpStatusCode statusCode)
        where TResponse : RestResponseAbstract
        => GetResponseBuilder(typeof(TResponse), statusCode);

    /// <summary>  
    /// Gets the map request for the specified request.  
    /// </summary>  
    /// <param name="request">The HTTP client request.</param>  
    /// <returns>The map request attribute for the specified request.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when the request is 
    /// not decorated with <see cref="MapRestAttribute"/> or 
    /// does not implement <see cref="IMapRestBuilder"/>.</exception>  
    public MapRestAbstractAttribute GetMapHttp(IRestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request is IMapRestBuilder builder
            ? builder.Build(this)
            : request
                .GetType()
                .GetCustomAttribute<MapRestAbstractAttribute>(true)
                ?? throw new InvalidOperationException(
                    $"Request must be decorated with one of the {nameof(MapRestAttribute)} " +
                    $"or implement {nameof(IMapRestBuilder)}");
    }

    /// <summary>
    /// Configures the default HTTP client options.
    /// </summary>
    /// <param name="options">The HTTP client options.</param>
    public static void Default(RestOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // response builders
        options.ResponseBuilders.Add(new RestResponseFailureBuilder());
        options.ResponseBuilders.Add(new RestResponseSuccessStreamBuilder());
        options.ResponseBuilders.Add(new RestResponseSuccessResultBuilder());
        options.ResponseBuilders.Add(new RestResponseSuccessBuilder());

        // request builders
        options.RequestBuilders.Add(new RestRequestPathStringBuilder());
        options.RequestBuilders.Add(new RestRequestQueryStringBuilder());
        options.RequestBuilders.Add(new RestRequestCookieBuilder());
        options.RequestBuilders.Add(new RestRequestHeaderBuilder());
        options.RequestBuilders.Add(new RestRequestBasicAuthenticationBuilder());
        options.RequestBuilders.Add(new RestRequestByteArrayBuilder());
        options.RequestBuilders.Add(new RestRequestFormUrlEncodedBuilder());
        options.RequestBuilders.Add(new RestRequestMultipartBuilder());
        options.RequestBuilders.Add(new RestRequestStreamBuilder());
        options.RequestBuilders.Add(new RestRequestPatchBuilder());
        options.RequestBuilders.Add(new RestRequestStringBuilder());

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
    public static RestOptions DefaultRestOptions
    {
        get
        {
            RestOptions options = new();
            Default(options);
            return options;
        }
    }
}
