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
using System.Net.Abstractions.Collections;
using System.Net.Http.Headers;

namespace System.Net.Rests;

/// <summary>
/// Defines a contract for providing HTTP Basic authentication credentials for REST requests.
/// </summary>
/// <remarks>Implementations of this interface generate authentication headers suitable for use with HTTP Basic
/// authentication schemes. This interface extends <see cref="IRestRequest"/>, allowing authentication information to be
/// integrated with REST request processing.</remarks>
public interface IRestBasicAuthentication : IRestRequest
{
    /// <summary>
    /// Returns the authentication header value.
    /// </summary>
    /// <returns>The authentication header value.</returns>
    AuthenticationHeaderValue GetAuthenticationHeaderValue();
}

/// <summary>
/// Defines a REST request that provides access to its content as a byte array.
/// </summary>
public interface IRestByteArray : IRestRequest
{
    /// <summary>
    /// Returns the byte array content.
    /// </summary>
    /// <returns>The byte array content.</returns>
    ByteArrayContent GetByteArrayContent();
}

/// <summary>
/// Defines the contract for a REST cookie that can be included in HTTP requests.
/// </summary>
public interface IRestCookie : IRestRequest
{
    /// <summary>
    /// Returns the cookie header value.
    /// </summary>
    /// <returns>The cookie header value.</returns>
    /// <remarks>If a key is already present, its value will be replaced with 
    /// the new one.</remarks>
    IDictionary<string, object?> GetCookieHeaderValue();
}

/// <summary>
/// Defines a contract for requests that provide form URL encoded content suitable for HTTP transmission.
/// </summary>
/// <remarks>Implementations of this interface allow clients to obtain a FormUrlEncodedContent instance
/// representing the request's data in application/x-www-form-urlencoded format. This is commonly used for submitting
/// form data in HTTP POST requests.</remarks>
public interface IRestFormUrlEncoded : IRestRequest
{
    /// <summary>
    /// Returns the form URL encoded content.
    /// </summary>
    /// <returns>The form URL encoded content.</returns>
    FormUrlEncodedContent GetFormUrlEncodedContent();
}

/// <summary>
/// Defines the contract for a REST header that provides access to a collection of headers and the name of the header
/// model.
/// </summary>
public interface IRestHeader : IRestRequest
{
    /// <summary>
    /// Gets the collection of headers.
    /// </summary>
    /// <returns>An <see cref="ElementCollection"/> containing the headers.</returns>
    ElementCollection GetHeaders();

    /// <summary>
    /// Gets the name of the header model.
    /// </summary>
    /// <returns>A string representing the name of the header model, 
    /// or null if not set.</returns>
    public string? GetHeaderModelName() => null;
}

/// <summary>
/// Defines a contract for REST requests that include multipart form data content.
/// </summary>
/// <remarks>Implementations of this interface provide access to multipart form data, typically used for file
/// uploads or forms containing both files and data fields. This interface extends <see cref="IRestRequest"/>, allowing
/// multipart content to be integrated with standard REST request handling.</remarks>
public interface IRestMultipart : IRestRequest
{
    /// <summary>
    /// Returns the multipart content.
    /// </summary>
    /// <returns>The multipart content.</returns>
    MultipartFormDataContent GetMultipartContent();
}

/// <summary>
/// Defines a contract for building a collection of key-value pairs representing the path components of a REST request
/// as strings.
/// </summary>
/// <remarks>Implementations of this interface provide a way to extract or construct the path portion of a RESTful
/// request in a structured format. This is typically used to facilitate routing, URL generation, or request analysis in
/// REST APIs.</remarks>
public interface IRestPathString : IRestRequest
{
    /// <summary>
    /// Returns the path string.
    /// </summary>
    /// <returns>The path string.</returns>
    IDictionary<string, string> GetPathString();
}

/// <summary>
/// Defines a contract for building and retrieving the query string parameters for a REST request.
/// </summary>
/// <remarks>Implementations of this interface provide access to the key-value pairs that represent the query
/// string of a RESTful HTTP request. This is typically used to append parameters to the request URL when interacting
/// with REST APIs.</remarks>
public interface IRestQueryString : IRestRequest
{
    /// <summary>
    /// Returns the query string.
    /// </summary>
    /// <returns>The query string.</returns>
    IDictionary<string, string?>? GetQueryString();
}

/// <summary>
/// Defines a REST request that provides access to stream-based content.
/// </summary>
[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
public interface IRestStream : IRestRequest
{
    /// <summary>
    /// Returns the stream content.
    /// </summary>
    /// <returns>The stream content.</returns>
    StreamContent GetStreamContent();
}

/// <summary>
/// Represents a REST request that provides access to string content.
/// </summary>
public interface IRestString : IRestRequest
{
    /// <summary>
    /// Returns the string content.
    /// </summary>
    /// <returns>The string content.</returns>
    /// <remarks>By default, the method returns the current instance.</remarks>
    public object GetStringContent() => this;
}

/// <summary>
/// Represents a REST request that applies a set of patch operations to a resource.
/// </summary>
public interface IRestPatch : IRestRequest
{
    /// <summary>
    /// Gets the patch operations.
    /// </summary>
    Collection<IPatchOperation> PatchOperations { get; }
}
