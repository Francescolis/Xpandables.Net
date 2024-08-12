/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using System.Text.Json;

using Xpandables.Net.Http.RequestBuilders;
using Xpandables.Net.Http.Requests;
using Xpandables.Net.Http.ResponseBuilders;

namespace Xpandables.Net.Http;

/// <summary>
/// Defines the <see cref="IHttpClientDispatcher"/>> configuration options.
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
    /// Sets the <see cref="IServiceProvider"/> to be used.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use.</param>
    public void SetServiceProvider(IServiceProvider serviceProvider)
        => ServiceProvider = serviceProvider;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> to be used.
    /// </summary>
    public IServiceProvider ServiceProvider { get; private set; } = default!;

    /// <summary>
    /// Resolves the response builder for the specified type.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response to resolve
    /// .</typeparam>
    /// <param name="targetStatusCode">The status code of the response.</param>
    /// <exception cref="InvalidOperationException">No response builder found for
    /// the specified type.</exception>
    public IHttpClientResponseBuilder GetResponseBuilderFor<TResponse>(
        HttpStatusCode targetStatusCode)
        => GetResponseBuilderFor(typeof(TResponse), targetStatusCode);

    /// <summary>
    /// Resolves the response builder for the specified type.
    /// </summary>
    /// <param name="targetType">The type of the interface implemented.</param>
    /// <param name="statusCode">The status code to act on.</param>
    public IHttpClientResponseBuilder GetResponseBuilderFor(
        Type targetType,
        HttpStatusCode statusCode)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        return ResponseBuilders
            .FirstOrDefault(x => x.CanBuild(targetType, statusCode))
            ?? throw new InvalidOperationException(
                $"No response builder registered for {targetType.Name}" +
                $"in the '{nameof(ResponseBuilders)}'.");
    }

    /// <summary>
    /// Resolves the request builder for the specified type.
    /// </summary>
    /// <param name="interfaceType">The type of the interface implemented.</param>
    public IHttpClientRequestBuilder GetRequestBuilderFor(Type interfaceType)
    {
        ArgumentNullException.ThrowIfNull(interfaceType);

        return RequestBuilders
            .FirstOrDefault(x => x.CanBuild(interfaceType))
            ?? throw new InvalidOperationException(
                $"No request builder registered for {interfaceType.Name}" +
                $"in the '{nameof(RequestBuilders)}'.");
    }

    /// <summary>
    /// Resolves the request builder for the specified type.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface 
    /// implemented.</typeparam>
    public IHttpClientRequestBuilder GetRequestBuilderFor<TInterface>()
        where TInterface : class, IHttpRequest
        => GetRequestBuilderFor(typeof(TInterface));

    /// <summary>
    /// Builds the default <see cref="HttpClientOptions"/> instance.
    /// </summary>
    /// <param name="options">The <see cref="HttpClientOptions"/> instance to 
    /// configure.</param>
    public static void Default(HttpClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _ = options.ResponseBuilders.Add(new HttpClientResponseSuccessBuilder());
        _ = options.ResponseBuilders.Add(new HttpClientResponseFailureBuilder());
        _ = options.ResponseBuilders.Add(new HttpClientResponseResultSuccessBuilder());
        _ = options.ResponseBuilders.Add(new HttpClientResponseResultFailureBuilder());
        _ = options.ResponseBuilders.Add(new HttpClientResponseAsyncResultSuccessBuilder());
        _ = options.ResponseBuilders.Add(new HttpClientResponseAsyncResultFailureBuilder());

        _ = options.RequestBuilders.Add(new HttpClientRequestPathBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestQueryStringBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestCookieBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestHeaderBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestBasicAuthBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestByteArrayBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestFormUrlEncodedBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestMultipartBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestStreamBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestStringBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestPatchBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestStartBuilder());
        _ = options.RequestBuilders.Add(new HttpClientRequestCompleteBuilder());

        options.SerializerOptions ??= new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = true
        };
    }
}