
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
using System.Net.Http.Headers;

using Xpandables.Net.Http.Builders;

using static Xpandables.Net.Http.MapRest;

namespace Xpandables.Net.Http;

/// <summary>
/// Abstract attribute to configure request definition.
/// The derived attribute should decorate implementations of <see cref="IRestRequest"/>,
/// <see cref="IRestStreamRequest{TResponse}"/> or <see cref="IRestRequest{TResponse}"/>
/// in order to be used with <see cref="IRestClient"/>.
/// </summary>
/// <remarks>
/// Your class can implement the <see cref="IMapRestBuilder"/>
/// to dynamically return a <see cref="MapRestAbstractAttribute"/>.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,
    Inherited = false, AllowMultiple = false)]
public abstract class MapRestAbstractAttribute : Attribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapRestAbstractAttribute"/>.
    /// </summary>
    protected MapRestAbstractAttribute() { }

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
    public BodyFormat BodyFormat { get; set; } = BodyFormat.String;

    /// <summary>
    /// Gets or sets the content type.
    /// The default value is <see cref="ContentType.Json"/>.
    /// </summary>
    public string ContentType { get; set; } = MapRest.ContentType.Json;

    /// <summary>
    /// Gets or sets the accept content.
    /// The default value is <see cref="ContentType.Json"/>.
    /// </summary>
    public string Accept { get; set; } = MapRest.ContentType.Json;

    /// <summary>
    /// Gets the value indicating whether or not the request needs authorization.
    /// The default value is <see langword="false"/>. If <see langword="true"/>,
    /// an <see cref="AuthenticationHeaderValue"/>
    /// with the <see cref="Scheme"/> value will be initialized and filled
    /// with an implementation of <see cref="RestAuthorizationHandler"/>.
    /// You need to configure the <see cref="IRestClient"/> 
    /// registration with one of the extension methods like
    /// <see langword="ConfigurePrimaryHttpMessageHandler{THandler}(IHttpClientBuilder)"/>    
    /// Or you can use a custom implementation to 
    /// fill the authentication header value.
    /// </summary>
    public bool IsSecured { get; set; }

    /// <summary>
    /// Gets or sets the authorization scheme.
    /// The default value is "Bearer".
    /// </summary>
    public string Scheme { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the request builder.
    /// If set, the request builder will be used to build the request.
    /// </summary>
    /// <remarks>Not yet implemented.</remarks>
    public IRestRequestBuilder? RequestBuilder { get; set; }

    /// <summary>
    /// Gets or sets the response builder.
    /// If set, the response builder will be used to build the response.
    /// </summary>
    /// <remarks>Not yet implemented.</remarks>
    public IRestResponseBuilder? ResponseBuilder { get; set; }

    // Gets or sets the built-in Uri.
    internal Uri Uri { get; set; } = null!;
}

/// <summary>
/// Attribute to configure request definition.
/// The attribute should decorate implementations of <see cref="IRestRequest"/>,
/// <see cref="IRestStreamRequest{TResponse}"/> or <see cref="IRestRequest{TResponse}"/>
/// in order to be used with <see cref="IRestClient"/>.
/// </summary>
/// <remarks>
/// Your class can implement the <see cref="IMapRestBuilder"/>
/// to dynamically return a <see cref="MapRestAttribute"/>.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,
    Inherited = false, AllowMultiple = false)]
public sealed class MapRestAttribute : MapRestAbstractAttribute { }

/// <summary>
/// Maps the request to the specified Uri path with the POST method.
/// </summary>
/// <remarks>The method is secured by default.</remarks>
public sealed class MapPostAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapPostAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapPostAttribute(string path)
    {
        Path = path;
        Method = Method.POST;
        IsSecured = true;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the GET method.
/// </summary>
public sealed class MapGetAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapGetAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapGetAttribute(string path)
    {
        Path = path;
        Method = Method.GET;
        Location = Location.Query;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the PUT method.
/// </summary>
/// <remarks>The method is secured by default.</remarks>
public sealed class MapPutAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapPutAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapPutAttribute(string path)
    {
        Path = path;
        Method = Method.PUT;
        IsSecured = true;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the DELETE method.
/// </summary>
/// <remarks>The method is secured by default.</remarks>
public sealed class MapDeleteAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapDeleteAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapDeleteAttribute(string path)
    {
        Path = path;
        Method = Method.DELETE;
        IsSecured = true;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the PATCH method.
/// </summary>
/// <remarks>The method is secured by default.</remarks>
public sealed class MapPatchAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapPatchAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapPatchAttribute(string path)
    {
        Path = path;
        Method = Method.PATCH;
        IsSecured = true;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the HEAD method.
/// </summary>
public sealed class MapHeadAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapHeadAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapHeadAttribute(string path)
    {
        Path = path;
        Method = Method.HEAD;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the OPTIONS method.
/// </summary>
public sealed class MapOptionsAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapOptionsAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapOptionsAttribute(string path)
    {
        Path = path;
        Method = Method.OPTIONS;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the TRACE method.
/// </summary>
public sealed class MapTraceAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapTraceAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapTraceAttribute(string path)
    {
        Path = path;
        Method = Method.TRACE;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the CONNECT method.
/// </summary>
public sealed class MapConnectAttribute : MapRestAbstractAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="MapConnectAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public MapConnectAttribute(string path)
    {
        Path = path;
        Method = Method.CONNECT;
    }
}