
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

// Ignore Spelling: Multipart

using System.Collections.ObjectModel;

using static Xpandables.Net.Http.HttpClientParameters;
using static Xpandables.Net.Http.HttpClientParameters.Patch;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides with a method to retrieve the request content 
/// for <see cref="BodyFormat.String"/> type.
/// </summary>
public interface IHttpRequestString
{
    /// <summary>
    /// Returns the body content that will be serialized.
    /// </summary>
    object GetStringContent();
}

/// <summary>
/// Defines an operation for the <see cref="Method.PATCH"/> method.
/// </summary>
public interface IPatchOperation
{
    internal PatchOperation GetOperation();
}

/// <summary>
/// Provides with a method to retrieve the request patch content 
/// for <see cref="BodyFormat.String"/> type.
/// You may use the <see cref="ContentType.JsonPatch"/> as content type.
/// <para>Use the <see cref="HttpRequestPatch{TRecord}"/> t
/// o implement the interface and
/// the <see cref="Patch"/> class to build operations.</para>
/// </summary>
/// <remarks>Note that there is no support for <see cref="Method.PATCH"/>
/// JsonPatch for minimal Api.</remarks>
public interface IHttpRequestPatch
{
    /// <summary>
    /// Returns the patch document.
    /// </summary>
    /// <remarks>The default behavior returns an empty collection.</remarks>
    public Collection<PatchOperation> PatchOperations => [];
}

/// <summary>
/// Helper used to implement the <see cref="IHttpRequestPatch"/> interface.
/// </summary>
/// <typeparam name="TRecord">The target patch record type.</typeparam>
public abstract record HttpRequestPatch<TRecord> : IHttpRequestPatch
    where TRecord : HttpRequestPatch<TRecord>
{
    /// <summary>
    /// Applies the <see cref="PatchOperationsBuilder"/> to the current instance.
    /// </summary>
    public Collection<PatchOperation> PatchOperations
        => PatchOperationsBuilder((TRecord)this);

    /// <summary>
    /// Provides with a method to build operations.
    /// </summary>
    public required Func<TRecord, Collection<PatchOperation>>
        PatchOperationsBuilder
    { get; init; }
}

/// <summary>
/// Provides with a method to retrieve the request content 
/// for <see cref="BodyFormat.ByteArray"/> type.
/// </summary>
public interface IHttpRequestByteArray
{
    /// <summary>
    /// Returns the body content.
    /// </summary>
    ByteArrayContent GetByteContent();
}

/// <summary>
/// Provides with a method to retrieve the request content 
/// for <see cref="BodyFormat.Multipart"/> type.
/// </summary>
public interface IHttpRequestMultipart
    : IHttpRequestStream, IHttpRequestString
{
    /// <summary>
    /// Returns the file name of the HTTP content to add.
    /// </summary>
    string GetFileName();

    /// <summary>
    /// Returns the name of the HTTP content to add.
    /// </summary>
    /// <remarks>The default value is 'file'.</remarks>
    public string GetName() => "file";
}

/// <summary>
/// Provides with a method to retrieve the request content 
/// for <see cref="BodyFormat.Stream"/> or <see cref="BodyFormat.Multipart"/> type.
/// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface IHttpRequestStream
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <summary>
    /// Returns the body content.
    /// </summary>
    StreamContent GetStreamContent();
}

/// <summary>
/// Provides with a method to retrieve the request 
/// content for <see cref="BodyFormat.FormUrlEncoded"/> type.
/// </summary>
public interface IHttpRequestFormUrlEncoded
{
    /// <summary>
    /// Returns the body content.
    /// </summary>
    IDictionary<string, string?> GetFormSource();
}

/// <summary>
/// Provides with a method to retrieve the request 
/// content for <see cref="Location.Cookie"/>.
/// </summary>
public interface IHttpRequestCookie
{
    /// <summary>
    /// Returns the keys and values for the cookie content.
    /// If a key is already present, its value will be replaced with the new one.
    /// </summary>
    IDictionary<string, object?> GetCookieSource();
}

/// <summary>
/// Provides with a method to retrieve the 
/// request content for <see cref="Location.Header"/>.
/// </summary>
public interface IHttpRequestHeader
{
    /// <summary>
    /// Returns the keys and values for the header content.
    /// If a key is already present, its value will be replaced with the new one.
    /// </summary>
    IDictionary<string, string?> GetHeaderSource();

    /// <summary>
    /// Returns the keys and values for the header content.
    /// If a key is already present, its value will be replaced with the new one.
    /// </summary>
    IDictionary<string, IEnumerable<string?>> GetHeadersSource()
        => GetHeaderSource()
        .ToDictionary(d => d.Key, d => (IEnumerable<string?>)[d.Value]);

    /// <summary>
    /// Returns the model name of the header attribute.
    /// </summary>
    /// <returns>The model name for the header attribute.</returns>
    public string? GetHeaderModelName() => null;
}

/// <summary>
/// Provides with a method to retrieve the basic authentication
/// content.
/// </summary>
public interface IHttpRequestBasicAuth
{
    /// <summary>
    /// Returns the basic authentication header.
    /// </summary>
    string GetBasicContent();
}

/// <summary>
/// Provides with a method to retrieve the query 
/// string content for query string Uri when using <see cref="Location.Query"/>.
/// This can be combined with other locations.
/// </summary>
public interface IHttpRequestQueryString
{
    /// <summary>
    /// Returns the keys and values for the query string Uri.
    /// </summary>
    IDictionary<string, string?>? GetQueryStringSource();
}

/// <summary>
/// Provides with a method to retrieve the path string content 
/// for query string Uri when using <see cref="Location.Path"/>.
/// This can be combined with other locations.
/// </summary>
public interface IHttpRequestPathString
{
    /// <summary>
    /// Returns the keys and values for the path string Uri.
    /// </summary>
    IDictionary<string, string> GetPathStringSource();
}
