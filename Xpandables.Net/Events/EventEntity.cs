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
using System.Text.Json;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents an abstract base class for event entities with a specific 
/// key and version.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public abstract class EventEntity<TKey> : Entity<TKey>, IEventEntity<TKey>
    where TKey : notnull, IComparable
{
    /// <inheritdoc/>
    public required string EventName { get; init; }

    /// <inheritdoc/>
    public required string EventFullName { get; init; }

    /// <inheritdoc/>
    public required JsonDocument EventData { get; init; }

    /// <inheritdoc/>
    public required ulong EventVersion { get; init; }

    private bool IsDisposed;

    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the EventEntity and optionally 
    /// releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged 
    /// resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            EventData?.Dispose();
        }

        // Dispose has been called.
        IsDisposed = true;
    }
}