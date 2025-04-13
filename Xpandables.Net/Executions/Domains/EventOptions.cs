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
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories.Converters;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Represents the options for configuring event handling.
/// </summary>
public sealed record EventOptions
{
    /// <summary>
    /// Gets the list of user-defined converters that were registered.
    /// </summary>
    public IList<IEventConverter> Converters { get; }
        =
        [
            new EventConverterDomain(),
            new EventConverterIntegration(),
            new EventConverterSnapshot()
        ];

    /// <summary>
    /// Returns the <see cref="IEventConverter"/> instance for the specified type.
    /// </summary>
    /// <param name="event">The event to convert.</param>
    /// <returns>The <see cref="IEventConverter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The converter was not 
    /// found.</exception>"
    public IEventConverter GetEventConverterFor(IEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return GetEventConverterFor(@event.GetType());
    }

    /// <summary>
    /// Returns the <see cref="IEventConverter"/> instance for the specified type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <returns>The <see cref="IEventConverter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The converter was not 
    /// found.</exception>"
    public IEventConverter GetEventConverterFor<TEvent>()
        where TEvent : class, IEvent
        => GetEventConverterFor(typeof(TEvent));

    /// <summary>
    /// Returns the <see cref="EventConverter"/> instance for the specified type.
    /// </summary>
    /// <param name="type">The type of event.</param>
    /// <returns>The <see cref="EventConverter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The converter was not 
    /// found.</exception>"
    public IEventConverter GetEventConverterFor(
        Type type)
        => Converters
            .FirstOrDefault(x => x.CanConvert(type))
            ?? throw new InvalidOperationException(
                $"The converter for the type '{type}' was not found.");
}
