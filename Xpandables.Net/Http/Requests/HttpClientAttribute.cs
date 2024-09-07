
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

// Ignore Spelling: Nullable

using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;

using static Xpandables.Net.Http.Requests.HttpClientParameters;

namespace Xpandables.Net.Http.Requests;

/// <summary>
/// Describes the parameters for a request used with 
/// <see cref="IHttpClientDispatcher"/>.
/// The attribute should decorate implementations of 
/// <see cref="IHttpClientRequest"/>,
/// <see cref="IHttpClientAsyncRequest{TResponse}"/> 
/// or <see cref="IHttpClientRequest{TResponse}"/>
/// in order to be used with <see cref="IHttpClientDispatcher"/>.
/// Your class can implement the <see cref="IHttpClientAttributeProvider"/>
/// to dynamically return a <see cref="HttpClientAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class HttpClientAttribute : Attribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="HttpClientAttribute"/>.
    /// </summary>
    public HttpClientAttribute() { }

    /// <summary>
    /// Gets or sets the Uri path. If null, the root path will be set.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the location of data.
    /// The default value is <see cref="Location.Body"/>.
    /// </summary>
    public Location Location { get; set; } = Location.Body;

    /// <summary>
    /// Gets or sets the method name.
    /// The default value is <see cref="Method.POST"/>.
    /// </summary>
    public Method Method { get; set; } = Method.POST;

    /// <summary>
    /// Gets or sets the format of the data.
    /// The default value is <see cref="DataFormat.Json"/>.
    /// </summary>
    public DataFormat DataFormat { get; set; }

    /// <summary>
    /// Gets or sets the body format for data.
    /// The default value is <see cref="BodyFormat.String"/>.
    /// </summary>
    public BodyFormat BodyFormat { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// The default value is <see cref="ContentType.Json"/>.
    /// </summary>
    public string ContentType { get; set; }
        = HttpClientParameters.ContentType.Json;

    /// <summary>
    /// Gets or sets the accept content.
    /// The default value is <see cref="ContentType.Json"/>.
    /// </summary>
    public string Accept { get; set; }
        = HttpClientParameters.ContentType.Json;

    /// <summary>
    /// Gets the value indicating whether or not the request needs authorization.
    /// The default value is <see langword="true"/>.
    /// In this case, an <see cref="AuthenticationHeaderValue"/>
    /// with the <see cref="Scheme"/> value will be initialized and filled
    /// with an implementation of <see cref="HttpClientAuthorizationHandler"/>.
    /// You need to configure the <see cref="IHttpClientDispatcher"/> 
    /// registration with one of the extension methods like
    /// <see cref="HttpClientBuilderExtensions
    /// .ConfigurePrimaryHttpMessageHandler{THandler}(IHttpClientBuilder)"/>    
    /// Or you can use a custom implementation to 
    /// fill the authentication header value.
    /// </summary>
    public bool IsSecured { get; set; } = true;

    /// <summary>
    /// Gets the value indicating whether or not the target 
    /// class should be added to the request body.
    /// If <see langword="true"/> the target class will not be added.
    /// The default value is <see langword="false"/>.
    /// Be aware of the fact that, setting this value to <see langword="true"/>
    /// will disable all parameters linked to <see cref="Location.Body"/>.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the authorization scheme.
    /// The default value is "Bearer".
    /// </summary>
    public string Scheme { get; set; } = "Bearer";

    // Gets or sets the built-in Uri.
    internal Uri Uri { get; set; } = null!;
}