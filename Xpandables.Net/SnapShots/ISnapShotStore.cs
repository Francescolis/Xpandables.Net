
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
using Xpandables.Net.Optionals;

namespace Xpandables.Net.SnapShots;

/// <summary>
/// Determines the <see cref="SnapShotOptions"/> status.
/// </summary>
public enum SnapShotStatus
{
    /// <summary>
    /// No use of snapshot to read/write.
    /// </summary>
    OFF = 0x0,

    /// <summary>
    /// Always use snapshot to read/write.
    /// </summary>
    ON = 0x1,
}

/// <summary>
/// Determines if the snapShot is enabled or not 
/// and the minimum of messages before creating one.
/// </summary>
public sealed record class SnapShotOptions
{
    /// <summary>
    /// Determines if the snapShot process status.
    /// </summary>
    public SnapShotStatus Status { get; init; } = SnapShotStatus.OFF;

    /// <summary>
    /// Determines the minimum of versions expected to create a snapShot.
    /// </summary>
    public ulong Frequency { get; init; } = 50;

    /// <summary>
    /// Determines if snapshot is used to read/write.
    /// </summary>
    public bool IsOn => Status == SnapShotStatus.ON;

    /// <summary>
    /// Determines if snapshot is off.
    /// </summary>
    public bool IsOff => Status == SnapShotStatus.OFF;
}

/// <summary>
/// Describes an originator to be converted to snapshot.
/// </summary>
/// <param name="Instance">The object instance.</param>
/// <param name="ObjectId">The object unique identifier of the instance.</param>
/// <param name="Version">The object version.</param>
public readonly record struct SnapShotDescriptor(
    IOriginator Instance,
    Guid ObjectId,
    ulong Version);

/// <summary>
/// Used to persist and read an object to/from an snapshot.
/// </summary>
public interface ISnapShotStore
{
    /// <summary>
    /// Asynchronously persists the object as snapshot according to configuration.
    /// </summary>
    /// <param name="descriptor">The descriptor object to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    ValueTask PersistAsSnapShotAsync(
        SnapShotDescriptor descriptor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns an object from the last snapshot matching the specified identifier.
    /// </summary>
    /// <typeparam name="T">the type of the expected object.</typeparam>
    /// <param name="objectId">The expected object identifier to search
    /// for.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an <see cref="Optional{T}"/>.</returns>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    ValueTask<Optional<T>> ReadFromSnapShotAsync<T>(
        Guid objectId,
        CancellationToken cancellationToken = default)
        where T : class, IOriginator;
}
