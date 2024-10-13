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
using System.Text.Json.Serialization;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Http;
/// <summary>
/// Defines an interface for building HTTP PATCH requests.
/// </summary>
public interface IHttpRequestPatch : IHttpRequestBuilder
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
/// Helper used to implement the <see cref="IHttpRequestPatch"/> interface.
/// </summary>
/// <typeparam name="TRecord">The target patch record type.</typeparam>
public abstract record HttpRequestPatch<TRecord> : IHttpRequestPatch
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