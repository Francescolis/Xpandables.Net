
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

using static Xpandables.Net.Executions.Rests.Rest;

namespace Xpandables.Net.Executions.Rests;

/// <summary>
/// Abstract attribute to configure request definition.
/// The derived attribute should decorate implementations of <see cref="IRestRequest"/>
/// in order to be used with <see cref="IRestClient"/>.
/// </summary>
/// <remarks>
/// Your class can implement the <see cref="IRestAttributeBuilder"/>
/// to dynamically return a <see cref="_RestAttribute"/>.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,
    Inherited = false, AllowMultiple = false)]
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable IDE1006 // Naming Styles
public abstract class _RestAttribute : Attribute
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1707 // Identifiers should not contain underscores
{
    /// <summary>
    /// Initializes the default instance of <see cref="_RestAttribute"/>.
    /// </summary>
    protected _RestAttribute() { }

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
    public string ContentType { get; set; } = Rest.ContentType.Json;

    /// <summary>
    /// Gets or sets the accept content.
    /// The default value is <see cref="ContentType.Json"/>.
    /// </summary>
    public string Accept { get; set; } = Rest.ContentType.Json;

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

    // Gets or sets the built-in Uri.
    internal Uri Uri { get; set; } = null!;
}

/// <summary>
/// Attribute to configure request definition.
/// The attribute should decorate implementations of <see cref="IRestRequest"/>
/// in order to be used with <see cref="IRestClient"/>.
/// </summary>
/// <remarks>
/// Your class can implement the <see cref="IRestAttributeBuilder"/>
/// to dynamically return a <see cref="RestAttribute"/>.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,
    Inherited = false, AllowMultiple = false)]
public sealed class RestAttribute : _RestAttribute { }

/// <summary>
/// Maps the request to the specified Uri path with the POST method.
/// </summary>
/// <remarks>The method is secured by default.</remarks>
public sealed class RestPostAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestPostAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestPostAttribute(string path)
    {
        Path = path;
        Method = Method.POST;
        IsSecured = true;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the GET method.
/// </summary>
public sealed class RestGetAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestGetAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestGetAttribute(string path)
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
public sealed class RestPutAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestPutAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestPutAttribute(string path)
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
public sealed class RestDeleteAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestDeleteAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestDeleteAttribute(string path)
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
public sealed class RestPatchAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestPatchAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestPatchAttribute(string path)
    {
        Path = path;
        Method = Method.PATCH;
        IsSecured = true;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the HEAD method.
/// </summary>
public sealed class RestHeadAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestHeadAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestHeadAttribute(string path)
    {
        Path = path;
        Method = Method.HEAD;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the OPTIONS method.
/// </summary>
public sealed class RestOptionsAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestOptionsAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestOptionsAttribute(string path)
    {
        Path = path;
        Method = Method.OPTIONS;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the TRACE method.
/// </summary>
public sealed class RestTraceAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestTraceAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestTraceAttribute(string path)
    {
        Path = path;
        Method = Method.TRACE;
    }
}

/// <summary>
/// Maps the request to the specified Uri path with the CONNECT method.
/// </summary>
public sealed class RestConnectAttribute : _RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestConnectAttribute"/>.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestConnectAttribute(string path)
    {
        Path = path;
        Method = Method.CONNECT;
    }
}