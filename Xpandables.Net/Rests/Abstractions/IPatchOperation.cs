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

namespace Xpandables.Net.Rests.Abstractions;

/// <summary>
/// Defines an operation for the PATCH method.
/// </summary>
public interface IPatchOperation
{
    internal PatchOperation GetOperation();
}

/// <summary>
/// Helper used to implement the <see cref="IRestPatch"/> interface.
/// </summary>
/// <typeparam name="TRecord">The target patch record type.</typeparam>
public abstract record RestContentPatch<TRecord> : IRestPatch
    where TRecord : RestContentPatch<TRecord>
{
    /// <summary>
    /// Applies the <see cref="PatchOperationsBuilder"/> to the current instance.
    /// </summary>
    public Collection<IPatchOperation> PatchOperations => PatchOperationsBuilder((TRecord)this);

    /// <summary>
    /// Provides with a method to build operations.
    /// </summary>
    public required Func<TRecord, Collection<IPatchOperation>> PatchOperationsBuilder { get; init; }
}

/// <summary>
/// Represents a single operation to be applied to a JSON document as part of a JSON Patch request.
/// </summary>
/// <remarks>A PatchOperation specifies the type of modification (such as add, remove, replace, move, or copy),
/// the target location within the JSON document, and, if applicable, the value to use or the source location. This type
/// is typically used to construct a sequence of operations that describe changes to be made to a JSON resource,
/// following the JSON Patch (RFC 6902) specification.</remarks>
public sealed record PatchOperation : IPatchOperation
{
    /// <summary>
    /// Gets the operation code associated with the message or request.
    /// </summary>
    [JsonPropertyName("op")]
    public string Op { get; init; } = default!;

    /// <summary>
    /// Gets the string containing the source location within the JSON document for operations that require it (e.g., move, copy).
    /// </summary>
    [JsonPropertyName("from"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? From { get; init; }

    /// <summary>
    /// Gets the target location within the JSON document where the operation is to be applied.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; init; }

    /// <summary>
    /// Gets the content value associated with the operation, if applicable.
    /// </summary>
    [JsonPropertyName("value"),
        JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public object? Value { get; init; }

    /// <summary>
    /// Initializes a new instance of the PatchOperation class with the specified operation type and target path.
    /// </summary>
    /// <param name="op">The type of patch operation to perform, such as "add", "remove", or "replace". Cannot be null or empty.</param>
    /// <param name="path">The JSON Pointer path that identifies the target location for the operation. Cannot be null or empty.</param>
    public PatchOperation(string op, string path)
    {
        Op = op;
        Path = path;
    }

    /// <summary>
    /// Initializes a new instance of the PatchOperation class with the specified operation type, target path, and
    /// value.
    /// </summary>
    /// <param name="op">The type of operation to perform, such as "add", "remove", or "replace". This value determines the kind of
    /// modification applied.</param>
    /// <param name="path">The target path within the document where the operation is to be applied. The format and interpretation of the
    /// path depend on the patch specification being used.</param>
    /// <param name="value">The value to use for the operation. This is required for operations that add or replace data, and may be ignored
    /// for others.</param>
    public PatchOperation(string op, string path, object value)
    {
        Op = op;
        Path = path;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the PatchOperation class with the specified operation type, source path, and
    /// target path.
    /// </summary>
    /// <param name="op">The type of patch operation to perform, such as "add", "remove", or "replace". Cannot be null or empty.</param>
    /// <param name="from">The source path for the operation, used in operations that require a source (such as "move" or "copy"). May be
    /// null for operations that do not require a source.</param>
    /// <param name="path">The target path where the operation will be applied. Cannot be null or empty.</param>
    public PatchOperation(string op, string from, string path)
    {
        Op = op;
        From = from;
        Path = path;
    }

    /// <summary>
    /// Initializes a new instance of the PatchOperation class with the specified operation type, source path, target
    /// path, and value.
    /// </summary>
    /// <param name="op">The operation type to perform, such as "add", "remove", or "replace". This value determines the kind of patch
    /// operation to be executed.</param>
    /// <param name="from">The source path for the operation, used by operations that require a source (such as "move" or "copy"). May be
    /// null or empty for operations that do not use a source path.</param>
    /// <param name="path">The target path where the operation will be applied. This specifies the location in the document to be modified.</param>
    /// <param name="value">The value to use for the operation, if applicable. For example, the value to add or replace at the target path.
    /// May be null for operations that do not require a value.</param>
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

    /// <summary>
    /// Returns the current patch operation instance.
    /// </summary>
    /// <returns>The current instance of the patch operation.</returns>
    PatchOperation IPatchOperation.GetOperation() => this;
}

/// <summary>
/// Provides a source generation context for serializing and deserializing collections of PatchOperation objects using
/// System.Text.Json.
/// </summary>
/// <remarks>This context enables efficient, compile-time generation of JSON serialization logic for the
/// Collection{PatchOperation} type. Use this context with System.Text.Json serialization APIs to improve performance
/// and reduce runtime reflection overhead when working with PatchOperation collections.</remarks>
[JsonSerializable(typeof(Collection<PatchOperation>))]
public sealed partial class PatchOperationJsonContext : JsonSerializerContext { }