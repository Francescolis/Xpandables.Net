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
using System.Text.Json;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents an abstract base class for event entities.
/// </summary>
public abstract class EventEntity : Entity<Guid>, IEventEntity
{
    /// <inheritdoc/>
    public required string EventName { get; init; }

    /// <inheritdoc/>
    public required string EventFullName { get; init; }

    /// <inheritdoc/>
    public required JsonDocument EventData { get; init; }

    /// <inheritdoc/>
    public required ulong EventVersion { get; init; }
}