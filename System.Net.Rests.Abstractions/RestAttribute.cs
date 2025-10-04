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

using static Xpandables.Net.Rests.RestSettings;

namespace Xpandables.Net.Rests;

/// <summary>
/// Specifies REST API metadata for a class or struct, including endpoint path, HTTP method, data format, and
/// authorization requirements.
/// </summary>
/// <remarks>Apply this attribute to a class or struct to define how it should be mapped to a RESTful endpoint.
/// The attribute allows configuration of the request path, HTTP method, data and body formats, content types, and
/// security settings. When the IsSecured property is set to <see langword="true"/>, the request will include an
/// authorization header using the specified scheme. This attribute is typically used in conjunction with a REST client
/// or framework that interprets these settings to construct and send HTTP requests.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
[Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
public class RestAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the Uri path. The default value is "/".
    /// </summary>
    public string Path { get; init; } = "/";

    /// <summary>
    /// Gets or sets the location of data.
    /// The default value is <see cref="Location.Body" />.
    /// </summary>
    public Location Location { get; set; } = Location.Body;

    /// <summary>
    /// Gets or sets the method name.
    /// The default value is <see cref="Method.POST" />.
    /// </summary>
    public Method Method { get; protected init; } = Method.POST;

    /// <summary>
    /// Gets or sets the format of the data.
    /// The default value is <see cref="DataFormat.Json" />.
    /// </summary>
    public DataFormat DataFormat { get; set; } = DataFormat.Json;

    /// <summary>
    /// Gets or sets the body format for data.
    /// The default value is <see cref="BodyFormat.String" />.
    /// </summary>
    public BodyFormat BodyFormat { get; set; } = BodyFormat.String;

    /// <summary>
    /// Gets or sets the content type.
    /// The default value is <see cref="ContentType.Json" />.
    /// </summary>
    public string ContentType { get; set; } = RestSettings.ContentType.Json;

    /// <summary>
    /// Gets or sets the accept content.
    /// The default value is <see cref="ContentType.Json" />.
    /// </summary>
    public string Accept { get; set; } = RestSettings.ContentType.Json;

    /// <summary>
    /// Gets the value indicating whether the request needs authorization.
    /// The default value is <see langword="false" />. If <see langword="true" />,
    /// an <see cref="AuthenticationHeaderValue" />
    /// with the <see cref="Scheme" /> value will be initialized and filled
    /// with a derived implementation of <see cref="RestAuthorizationHandler" />.
    /// You need to configure the <see cref="IRestClient" />
    /// registration with one of the extension methods like
    /// <see langword="ConfigurePrimaryHttpMessageHandler{THandler}(IHttpClientBuilder)" />
    /// Or you can use a custom implementation to
    /// fill the authentication header value.
    /// </summary>
    public bool IsSecured { get; set; }

    /// <summary>
    /// Gets or sets the authorization scheme.
    /// The default value is "Bearer".
    /// </summary>
    public string Scheme { get; set; } = "Bearer";
}

/// <summary>
/// Specifies that a method should be mapped to an HTTP POST request at the specified URI path.
/// </summary>
/// <remarks>Use this attribute to indicate that a controller or handler method responds to HTTP POST requests for
/// the given path. The attribute sets the HTTP method to POST and marks the endpoint as secured by default. This
/// attribute is typically used in RESTful APIs to handle resource creation or actions that modify server
/// state.</remarks>
public sealed class RestPostAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestPostAttribute" />.
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
/// Specifies that a method should be mapped to an HTTP GET request in a RESTful API.
/// </summary>
/// <remarks>Apply this attribute to a method to indicate that it handles HTTP GET requests for the specified URI
/// path. This attribute is typically used in REST API client libraries to associate interface methods with GET
/// operations. The request parameters are sent as query string values by default.</remarks>
public sealed class RestGetAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestGetAttribute" />.
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
/// Specifies that a method responds to HTTP PUT requests in a RESTful API and defines the associated URI path.
/// </summary>
/// <remarks>Apply this attribute to a controller method to indicate that it should handle HTTP PUT requests for
/// the specified path. This attribute also marks the method as secured by default.</remarks>
public sealed class RestPutAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestPutAttribute" />.
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
/// Specifies that a method responds to HTTP DELETE requests at the specified URI path.
/// </summary>
/// <remarks>Apply this attribute to a controller method to indicate that it should handle HTTP DELETE requests
/// for the given path. This attribute is typically used in RESTful APIs to map DELETE operations to specific endpoints.
/// The request will be secured by default.</remarks>
public sealed class RestDeleteAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestDeleteAttribute" />.
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
/// Specifies that a method handles HTTP PATCH requests in a RESTful API.
/// </summary>
/// <remarks>Apply this attribute to a controller method to indicate that it should respond to HTTP PATCH requests
/// at the specified URI path. This attribute is typically used in web frameworks that support attribute-based routing
/// for REST APIs.</remarks>
public sealed class RestPatchAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestPatchAttribute" />.
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
/// Specifies that a method should be mapped to an HTTP HEAD request in a RESTful API.
/// </summary>
/// <remarks>Apply this attribute to a method to indicate that it handles HTTP HEAD requests for the specified URI
/// path. The HEAD method is used to retrieve metadata about a resource without returning the resource body. This
/// attribute is typically used in REST API client libraries to declaratively map interface methods to HTTP HEAD
/// operations.</remarks>
public sealed class RestHeadAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestHeadAttribute" />.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestHeadAttribute(string path)
    {
        Path = path;
        Method = Method.HEAD;
    }
}

/// <summary>
/// Specifies that a controller action supports the HTTP OPTIONS method for the specified route path.
/// </summary>
public sealed class RestOptionsAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestOptionsAttribute" />.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestOptionsAttribute(string path)
    {
        Path = path;
        Method = Method.OPTIONS;
    }
}

/// <summary>
/// Specifies that a method responds to HTTP TRACE requests at the specified URI path.
/// </summary>
public sealed class RestTraceAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestTraceAttribute" />.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestTraceAttribute(string path)
    {
        Path = path;
        Method = Method.TRACE;
    }
}

/// <summary>
/// Specifies that a method handles HTTP CONNECT requests for the specified URI path.
/// </summary>
public sealed class RestConnectAttribute : RestAttribute
{
    /// <summary>
    /// Initializes the default instance of <see cref="RestConnectAttribute" />.
    /// </summary>
    /// <param name="path">The Uri path.</param>
    public RestConnectAttribute(string path)
    {
        Path = path;
        Method = Method.CONNECT;
    }
}