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
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

using Xpandables.Net.Collections;

using static Xpandables.Net.Http.MapRequest;

namespace Xpandables.Net.Http;

/// <summary>
/// Specifies the content of an HTTP request.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IHttpRequestContent { }
#pragma warning restore CA1040 // Avoid empty interfaces

/// <summary>
/// Specifies that the HTTP request content is a basic authentication.
/// </summary>
public interface IHttpRequestContentBasicAuthentication : IHttpRequestContent
{
    /// <summary>
    /// Returns the authentication header value.
    /// </summary>
    /// <returns>The authentication header value.</returns>
    AuthenticationHeaderValue GetAuthenticationHeaderValue();
}

/// <summary>
/// Specifies that the HTTP request content is a byte array.
/// </summary>
public interface IHttpRequestContentByteArray : IHttpRequestContent
{
    /// <summary>
    /// Returns the byte array content.
    /// </summary>
    /// <returns>The byte array content.</returns>
    ByteArrayContent GetByteArrayContent();
}

/// <summary>
/// Specifies that the HTTP request content is a cookie.
/// </summary>
public interface IHttpRequestContentCookie : IHttpRequestContent
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
/// Specifies that the HTTP request content is a form URL encoded.
/// </summary>
public interface IHttpRequestContentFormUrlEncoded : IHttpRequestContent
{
    /// <summary>
    /// Returns the form URL encoded content.
    /// </summary>
    /// <returns>The form URL encoded content.</returns>
    FormUrlEncodedContent GetFormUrlEncodedContent();
}

/// <summary>
/// Specifies that the HTTP request content is a header.
/// </summary>
/// <remarks>if you want to set the header model name, you can override the 
/// <see cref="GetHeaderModelName"/> method.</remarks>
public interface IHttpRequestContentHeader : IHttpRequestContent
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
/// Specifies that the HTTP request content is a multipart content.
/// </summary>
public interface IHttpRequestContentMultipart : IHttpRequestContent
{
    /// <summary>
    /// Returns the multipart content.
    /// </summary>
    /// <returns>The multipart content.</returns>
    MultipartFormDataContent GetMultipartContent();
}

/// <summary>
/// Specifies that the HTTP request content is a path string.
/// </summary>
public interface IHttpRequestContentPathString : IHttpRequestContent
{
    /// <summary>
    /// Returns the path string.
    /// </summary>
    /// <returns>The path string.</returns>
    IDictionary<string, string> GetPathString();
}

/// <summary>
/// Specifies that the HTTP request content is a query string.
/// </summary>
public interface IHttpRequestContentQueryString : IHttpRequestContent
{
    /// <summary>
    /// Returns the query string.
    /// </summary>
    /// <returns>The query string.</returns>
    IDictionary<string, string?>? GetQueryString();
}

/// <summary>
/// Specifies that the HTTP request content is a stream.
/// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface IHttpRequestContentStream : IHttpRequestContent
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <summary>
    /// Returns the stream content.
    /// </summary>
    /// <returns>The stream content.</returns>
    StreamContent GetStreamContent();
}

/// <summary>
/// Specifies that the HTTP request content is a json string.
/// </summary>
public interface IHttpRequestContentString : IHttpRequestContent
{
    /// <summary>
    /// Returns the string content.
    /// </summary>
    /// <returns>The string content.</returns>
    /// <remarks>By default, the method returns the current instance.</remarks>
    public object GetStringContent() => this;
}

/// <summary>
/// Specifies that the HTTP request content is a patch operation.
/// </summary>
public interface IHttpRequestContentPatch : IHttpRequestContent
{
    /// <summary>
    /// Gets the patch operations.
    /// </summary>
    Collection<IPatchOperation> PatchOperations { get; }
}

/// <summary>
/// Defines an operation for the <see cref="Method.PATCH"/> method.
/// </summary>
public interface IPatchOperation
{
    internal PatchOperation GetOperation();
}

/// <summary>
/// Helper used to implement the <see cref="IHttpRequestContentPatch"/> interface.
/// </summary>
/// <typeparam name="TRecord">The target patch record type.</typeparam>
public abstract record HttpRequestPatch<TRecord> : IHttpRequestContentPatch
    where TRecord : HttpRequestPatch<TRecord>
{
    /// <summary>
    /// Applies the <see cref="PatchOperationsBuilder"/> to the current instance.
    /// </summary>
    public Collection<IPatchOperation> PatchOperations
        => PatchOperationsBuilder((TRecord)this);

    /// <summary>
    /// Provides with a method to build operations.
    /// </summary>
    public required Func<TRecord, Collection<IPatchOperation>>
        PatchOperationsBuilder
    { get; init; }
}

internal sealed record PatchOperation : IPatchOperation
{
    [JsonPropertyName("op")]
    public string Op { get; init; } = default!;

    [JsonPropertyName("from"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? From { get; init; }

    [JsonPropertyName("path")]
    public string Path { get; init; }

    [JsonPropertyName("value"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public object? Value { get; init; }

    public PatchOperation(string op, string path)
    {
        Op = op;
        Path = path;
    }
    public PatchOperation(string op, string path, object value)
    {
        Op = op;
        Path = path;
        Value = value;
    }
    public PatchOperation(string op, string from, string path)
    {
        Op = op;
        From = from;
        Path = path;
    }

    public PatchOperation(
        string op,
        string from,
        string path,
        object? value)
    {
        Op = op;
        From = from;
        Path = path;
        Value = value;
    }

    PatchOperation IPatchOperation.GetOperation() => this;
}