
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

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents an abstract base class for event entities.
/// </summary>
public abstract class EntityEvent : Entity<Guid>, IEntityEvent
{
    /// <inheritdoc />
    public required string Name { get; init; }

    /// <inheritdoc />
    public required string FullName { get; init; }

    /// <inheritdoc />
    public required JsonDocument Data { get; init; }

    /// <inheritdoc />
    public long Sequence { get; init; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the event entity.
    /// </summary>
    /// <param name="disposing">
    /// True if the method is called directly or indirectly by user code; false if called by the
    /// runtime from within the finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Data?.Dispose();
        }
    }
}