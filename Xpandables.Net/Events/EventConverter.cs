
/*******************************************************************************
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
********************************************************************************/
using System.Text.Json;

namespace Xpandables.Net.Events;

/// <summary>
/// Converts an event to or from entity.
/// </summary>
public abstract class EventConverter : IEventConverter
{
    /// <inheritdoc/>
    public abstract Type Type { get; }

    /// <inheritdoc/>
    public abstract bool CanConvert(Type typeToConvert);

    /// <inheritdoc/>
    public abstract IEntityEvent ConvertTo(
        IEvent @event,
        JsonSerializerOptions? options = default);

    /// <inheritdoc/>
    public abstract IEvent ConvertFrom(
        IEntityEvent entity,
        JsonSerializerOptions? options = default);
}