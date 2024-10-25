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

using Xpandables.Net.Events.Converters;
using Xpandables.Net.Text;

namespace Xpandables.Net.Events;
/// <summary>
/// Represents the options for configuring event handling.
/// </summary>
public sealed record EventOptions
{
    /// <summary>
    /// Gets the list of user-defined converters that were registered.
    /// </summary>
    public IList<IEventConverter> Converters { get; init; }
        = [];

    /// <summary>
    /// Gets the JSON serializer options.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; init; } =
        DefaultSerializerOptions.Defaults;

    /// <summary>
    /// Gets a value indicating whether snapshot is enabled.
    /// </summary>
    public bool IsSnapshotEnabled { get; init; } = true;

    /// <summary>
    /// Gets the frequency of snapshots.
    /// </summary>
    public ulong SnapshotFrequency { get; init; } = 50;

    /// <summary>
    /// Gets a value indicating whether the event scheduler is enabled.
    /// </summary>
    public bool IsEventSchedulerEnabled { get; init; } = true;

    /// <summary>
    /// Gets the maximum number of retries for the scheduler.
    /// </summary>
    public uint MaxSchedulerRetries { get; init; } = 5;

    /// <summary>
    /// Gets the interval between scheduler retries in milliseconds.
    /// </summary>
    public uint SchedulerRetryInterval { get; init; } = 500;

    /// <summary>
    /// Gets the maximum number of events per thread for the scheduler.
    /// </summary>
    public ushort MaxSchedulerEventPerThread { get; init; } = 100;

    /// <summary>
    /// Returns the <see cref="IEventConverter"/> instance for the specified type.
    /// </summary>
    /// <param name="event">The event to convert.</param>
    /// <returns>The <see cref="IEventConverter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The converter was not 
    /// found.</exception>"
    public IEventConverter GetEventConverterFor(IEvent @event) =>
        GetEventConverterFor(@event.GetType());

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


    /// <summary>
    /// Returns the default <see cref="EventOptions"/>.
    /// </summary>
    /// <param name="options">The options to use as a base for the default values.</param>
    /// <returns>The default <see cref="EventOptions"/>.</returns>
    public static void Default(EventOptions options)
    {
        options = options with
        {
            SerializerOptions = DefaultSerializerOptions.Defaults,
            IsSnapshotEnabled = true,
            SnapshotFrequency = 50,
            IsEventSchedulerEnabled = true,
            MaxSchedulerRetries = 5,
            SchedulerRetryInterval = 500,
            MaxSchedulerEventPerThread = 100
        };

        options.Converters.Add(new EventConverterDomain());
        options.Converters.Add(new EventConverterIntegration());
        options.Converters.Add(new EventConverterSnapshot());
    }
}
