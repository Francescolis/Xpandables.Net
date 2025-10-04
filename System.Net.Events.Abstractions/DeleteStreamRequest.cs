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
namespace System.Net.Events;

/// <summary>
/// Represents a request to delete an event stream, specifying the stream identifier and whether to perform a hard or
/// soft delete.
/// </summary>
/// <remarks>Use this type to encapsulate the parameters required when requesting the deletion of an event stream.
/// A hard delete permanently removes the stream and its data, while a soft delete marks the stream as deleted but may
/// allow for recovery or auditing, depending on the system's implementation.</remarks>
public readonly record struct DeleteStreamRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteStreamRequest"/> class.
    /// </summary>
    public DeleteStreamRequest()
    {
    }
    /// <summary>
    /// Gets the identifier of the event stream to delete.
    /// </summary>
    public readonly required Guid StreamId { get; init; }

    /// <summary>
    /// Gets a value indicating whether to perform a hard delete (true) or a soft delete (false).
    /// </summary>
    public readonly required bool HardDelete { get; init; } = false;
}
