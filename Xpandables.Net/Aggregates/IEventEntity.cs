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
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents an event entity.
/// </summary>
public interface IEventEntity : IEntity<Guid>, IDisposable
{
    /// <summary>
    /// Contains the string representation of the .Net event type name.
    /// </summary>
    string EventTypeName { get; }

    /// <summary>
    /// Contains the string representation of the .Net event 
    /// full assembly qualified type name.
    /// </summary>
    string EventTypeFullName { get; }

    /// <summary>
    /// Gets the representation of the object version.
    /// </summary>
    [ConcurrencyCheck]
    ulong Version { get; }

    /// <summary>
    /// Contains the representation of the event as <see cref="JsonDocument"/>.
    /// </summary>
    JsonDocument Data { get; }

    /// <summary>
    /// Converts the event entity to the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to convert to.</typeparam>
    /// <param name="entity">The event entity to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The event entity converted to the specified event type.</returns>
    /// <exception cref="InvalidOperationException">Failed to convert the event
    /// entity.</exception>
    public static TEvent? ConvertFrom<TEvent>(
        IEventEntity entity,
        JsonSerializerOptions? options = default)
        where TEvent : class, IEventEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            if (Type.GetType(entity.EventTypeFullName)
                is not { } eventType)
                return default;

            object? result = entity.Data.Deserialize(eventType, options);

            return result as TEvent;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                        and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                I18nXpandables.ActionSpecifiedFailedSeeException
                    .StringFormat(nameof(ConvertFrom)),
                exception);
        }
    }
}
