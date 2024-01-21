
/************************************************************************************************************
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
************************************************************************************************************/
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using Xpandables.Net.Primitives.Text;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.SnapShots;

/// <summary>
/// Represents a snapshot to be written.
/// Make use of <see langword="using"/> key work when call or call dispose method.
/// </summary>
public sealed class SnapShotRecord : Entity<Guid>, IDisposable
{
    /// <summary>
    /// Constructs a snapshot record from the specified snapshot.
    /// </summary>
    /// <param name="descriptor">The descriptor originator to act with.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>An instance of snapshot entity built from the snapshot.</returns>
    public static SnapShotRecord FromSnapShotDescriptor(
        SnapShotDescriptor descriptor,
        JsonSerializerOptions options)
    {
        Guid objectId = descriptor.ObjectId;
        ulong version = descriptor.Version;
        string name = descriptor.Instance.GetTypeName();
        IMemento memento = descriptor.Instance.CreateMemento();
        string mementoTypeName = memento.GetTypeName();

        JsonDocument data = memento.ToJsonDocument(options);

        return new(objectId, version, name, mementoTypeName, data);
    }

    /// <summary>
    /// Converts a record to a <see cref="IMemento"/>.
    /// </summary>
    /// <param name="record">The record to act with.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>An instance of event built from the entity.</returns>
    public static IMemento? ToMemento(
        SnapShotRecord record,
        JsonSerializerOptions? options)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (Type.GetType(record.MementoTypeName) is { } mementoType)
            if (record.Data.Deserialize(mementoType, options) is IMemento memento)
                return memento;

        return default;
    }

    ///<inheritdoc/>
    private SnapShotRecord(
        Guid objectId,
        ulong version,
        string objectTypeName,
        string mementoTypeName,
        JsonDocument data)
    {
        Id = Guid.NewGuid();
        ObjectId = objectId;
        ObjectTypeName = objectTypeName;
        MementoTypeName = mementoTypeName;
        Version = version;
        Data = data;
    }

    /// <summary>
    /// Gets the object identifier.
    /// </summary>
    [Key]
    public Guid ObjectId { get; }

    /// <summary>
    /// Gets the representation of the object version.
    /// </summary>
    [ConcurrencyCheck]
    public ulong Version { get; }

    /// <summary>
    /// Gets the object type name.
    /// </summary>
    public string ObjectTypeName { get; }

    /// <summary>
    /// Gets the memento type name.
    /// </summary>
    public string MementoTypeName { get; }

    /// <summary>
    /// Contains the representation of the event as <see cref="JsonDocument"/>.
    /// </summary>
    public JsonDocument Data { get; }

    /// <summary>
    /// Releases the <see cref="Data"/> resource.
    /// </summary>
    public void Dispose()
    {
        Data?.Dispose();
        GC.SuppressFinalize(this);
    }
}
