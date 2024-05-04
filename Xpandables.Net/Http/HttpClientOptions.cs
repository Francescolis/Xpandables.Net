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

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Http.Builders;
using Xpandables.Net.Http.Builders.Requests;
using Xpandables.Net.Http.Builders.Responses;

namespace Xpandables.Net.Http;

/// <summary>
/// Defines the <see cref="IHttpClientDispatcher"/>> configuration options.
/// </summary>
public sealed record HttpClientOptions
{
    /// <summary>
    /// Gets the list of user-defined response builders that were registered.
    /// </summary>
    public IDictionary<Type, List<Type>> ResponseBuilders { get; }
        = new Dictionary<Type, List<Type>>();

    /// <summary>
    /// Gets the list of user-defined request builders that were registered.
    /// </summary>
    public IList<HttpClientRequestBuilder> RequestBuilders { get; }
        = new List<HttpClientRequestBuilder>();

    /// <summary>
    /// Gets or sets the attribute builder to be used.
    /// </summary>
    public HttpClientRequestAttributeBuilder RequestAttributeBuilder { get; set; }
        = new HttpClientAttributeBuilder();

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
    public object GetHttpClientResponseBuilder(
        IServiceProvider serviceProvider,
        HttpStatusCode targetStatusCode,
        Type? resultType = default)
    {
        Type type = GetHttpResponseInterfaceType(resultType);

        return serviceProvider
            .GetServices(type)
            .OfType<IHttpClientResponseBuilderBase>()
            .FirstOrDefault(sce => sce.CanBuild(targetStatusCode, resultType))
            ?? throw new InvalidOperationException(
                $"No response builder found for {resultType?.Name}.");
    }

    /// <summary>
    /// Resolves the request builder for the specified type.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface 
    /// implemented.</typeparam>
    public IHttpClientRequestBuilder<TInterface>
        GetHttpClientRequestBuilder<TInterface>()
        where TInterface : class
        => RequestBuilders
            .FirstOrDefault(x => x.CanBuild(typeof(TInterface)))
            .As<IHttpClientRequestBuilder<TInterface>>()
            ?? throw new InvalidOperationException(
                $"No request builder found for {typeof(TInterface).Name}.");

    /// <summary>
    /// Builds the default <see cref="HttpClientOptions"/> instance.
    /// </summary>
    /// <param name="options">The <see cref="HttpClientOptions"/> instance to 
    /// configure.</param>
    public static void Default(HttpClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<Type> emptyResponses =
            [
                typeof(SuccessHttpClientResponseBuilder),
                typeof(FailureHttpClientResponseBuilder)
            ];

        options
            .ResponseBuilders
            .Add(typeof(IHttpClientResponseBuilder), emptyResponses);

        List<Type> resultResponses =
            [
                typeof(SuccessHttpClientResponseResultBuilder<>),
                typeof(FailureHttpClientResponseResultBuilder<>)
            ];

        options
            .ResponseBuilders
            .Add(typeof(IHttpClientResponseResultBuilder<>), resultResponses);

        List<Type> iasyncResultResponses =
            [
                typeof(SuccessHttpClientResponseAsyncResultBuilder<>),
                typeof(FailureHttpClientResponseAsyncResultBuilder<>)
            ];

        options
            .ResponseBuilders
            .Add(typeof(IHttpClientResponseIAsyncResultBuilder<>), iasyncResultResponses);


        options.RequestBuilders.Add(new HttpClientRequestPathBuilder());
        options.RequestBuilders.Add(new HttpClientRequestQueryStringBuilder());
        options.RequestBuilders.Add(new HttpClientRequestCookieBuilder());
        options.RequestBuilders.Add(new HttpClientRequestHeaderBuilder());
        options.RequestBuilders.Add(new HttpClientRequestByteArrayBuilder());
        options.RequestBuilders.Add(new HttpClientRequestFormUrlEncodedBuilder());
        options.RequestBuilders.Add(new HttpClientRequestMultipartBuilder());
        options.RequestBuilders.Add(new HttpClientRequestStreamBuilder());
        options.RequestBuilders.Add(new HttpClientRequestStringBuilder());
        options.RequestBuilders.Add(new HttpClientRequestPatchBuilder());

        options.SerializerOptions ??= new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null
            //WriteIndented = true
        };
    }

    private Type GetHttpResponseInterfaceType(Type? type)
    {
        Type? resultType = default;

        if (type is null)
            resultType = ResponseBuilders
                .Keys
                .FirstOrDefault(t => t == typeof(IHttpClientResponseBuilder));

        if (type is not null && !type.IsGenericType)
            resultType = ResponseBuilders
                .Keys
                .FirstOrDefault(t =>
                    t == typeof(IHttpClientResponseResultBuilder<>))
                ?.MakeGenericType(type);

        if (type is not null && type.IsGenericType)
        {
            Type[] types = type.GetGenericArguments();
            Type iasyncType = types[0];
            resultType = ResponseBuilders
                .Keys
                .FirstOrDefault(t =>
                    t == typeof(IHttpClientResponseIAsyncResultBuilder<>))
                ?.MakeGenericType(iasyncType);
        }

        return resultType ??
            throw new InvalidOperationException(
                $"No response builder found for {type?.Name}.");
    }
}