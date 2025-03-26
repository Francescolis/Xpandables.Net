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

using static Xpandables.Net.Http.RequestDefinitions;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides with the base HTTP content request definition.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IRequestHttpDefinition { }
#pragma warning restore CA1040 // Avoid empty interfaces

/// <summary>
/// Provides with base HTTP content used to start building a request.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IRequestHttpStart : IRequestHttpDefinition { }
#pragma warning restore CA1040 // Avoid empty interfaces

/// <summary>
/// Provides with base HTTP content used to complete building a request.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IRequestHttpCompletion : IRequestHttpDefinition { }
#pragma warning restore CA1040 // Avoid empty interfaces

/// <summary>
/// Interface for building HTTP requests with basic authentication.
/// </summary>
public interface IRequestBasicAuthentication : IRequestHttpDefinition
{
    /// <summary>
    /// Returns the authentication header value.
    /// </summary>
    /// <returns>The authentication header value.</returns>
    AuthenticationHeaderValue GetAuthenticationHeaderValue();
}

/// <summary>
/// Represents an HTTP request that contains a byte array content.
/// </summary>
public interface IRequestByteArray : IRequestHttpDefinition
{
    /// <summary>
    /// Returns the byte array content.
    /// </summary>
    /// <returns>The byte array content.</returns>
    ByteArrayContent GetByteArrayContent();
}

/// <summary>
/// Represents an HTTP request cookie builder.
/// </summary>
public interface IRequestCookie : IRequestHttpDefinition
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
/// Interface for building HTTP requests with form URL encoded content.
/// </summary>
public interface IRequestFormUrlEncoded : IRequestHttpDefinition
{
    /// <summary>
    /// Returns the form URL encoded content.
    /// </summary>
    /// <returns>The form URL encoded content.</returns>
    FormUrlEncodedContent GetFormUrlEncodedContent();
}

/// <summary>
/// Interface for building HTTP Header requests.
/// </summary>
/// <remarks>if you want to set the header model name, you can override the 
/// <see cref="GetHeaderModelName"/> method.</remarks>
public interface IRequestHeader : IRequestHttpDefinition
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
/// Represents an HTTP request that supports multipart content.
/// </summary>
public interface IRequestMultipart : IRequestHttpDefinition
{
    /// <summary>
    /// Returns the multipart content.
    /// </summary>
    /// <returns>The multipart content.</returns>
    MultipartFormDataContent GetMultipartContent();
}

/// <summary>
/// Interface for building HTTP request path strings.
/// </summary>
public interface IRequestPathString : IRequestHttpDefinition
{
    /// <summary>
    /// Returns the path string.
    /// </summary>
    /// <returns>The path string.</returns>
    IDictionary<string, string> GetPathString();
}

/// <summary>
/// Interface for building HTTP request query strings.
/// </summary>
public interface IRequestQueryString : IRequestHttpDefinition
{
    /// <summary>
    /// Returns the query string.
    /// </summary>
    /// <returns>The query string.</returns>
    IDictionary<string, string?>? GetQueryString();
}

/// <summary>
/// Represents an HTTP request stream that builds the request and provides 
/// the stream content.
/// </summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public interface IRequestStream : IRequestHttpDefinition
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    /// <summary>
    /// Returns the stream content.
    /// </summary>
    /// <returns>The stream content.</returns>
    StreamContent GetStreamContent();
}

/// <summary>
/// Interface for building HTTP request strings.
/// </summary>
public interface IRequestString : IRequestHttpDefinition
{
    /// <summary>
    /// Returns the string content.
    /// </summary>
    /// <returns>The string content.</returns>
    /// <remarks>By default, the method returns the current instance.</remarks>
    public object GetStringContent() => this;
}

/// <summary>
/// Defines an interface for building HTTP PATCH requests.
/// </summary>
public interface IRequestPatch : IRequestHttpDefinition
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
/// Helper used to implement the <see cref="IRequestPatch"/> interface.
/// </summary>
/// <typeparam name="TRecord">The target patch record type.</typeparam>
public abstract record HttpRequestPatch<TRecord> : IRequestPatch
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