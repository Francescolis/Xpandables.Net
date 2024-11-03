﻿/*******************************************************************************
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
namespace Xpandables.Net.Events;
/// <summary>
/// Represents a domain event that includes versioning and an aggregate identifier.
/// </summary>
public interface IEventDomain : IEvent
{
    /// <summary>
    /// Sets the version of the event.
    /// </summary>
    /// <param name="version">The version to set.</param>
    /// <returns>The event domain with the specified version.</returns>
    IEventDomain WithVersion(ulong version);

    /// <summary>
    /// Gets the aggregate identifier associated with the event.
    /// </summary>
    /// <remarks>It's based on the <see cref="Guid.NewGuid"/>.</remarks>
    Guid AggregateId { get; init; }
}