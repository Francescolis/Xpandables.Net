
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

using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Events;

/// <summary>
/// Defines the event configuration options.
/// </summary>
public sealed record EventOptions
{
    /// <summary>
    /// Gets the list of user-defined converters that were registered.
    /// </summary>
    public IList<IEventConverter> Converters { get; }
        = [];
    /// <summary>
    /// Gets the list of user-defined filters that were registered.
    /// </summary>
    public IList<IEventFilter> Filters { get; }
        = [];

    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> to be used.
    /// </summary>
    public JsonSerializerOptions? SerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the snapshot options.
    /// </summary>
    public SnapshotOptions SnapshotOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the integration event options.
    /// </summary>
    public SchedulerOptions SchedulerOptions { get; set; } = new();

    /// <summary>
    /// Determines whether to dispose entity event after persistence.
    /// </summary>
    /// <remarks>The default value is <see langword="true"/>.
    /// <para>This flag is useful when you use the built in event persistence 
    /// model that contains a disposable <see cref="JsonDocument"/> property.
    /// </para></remarks>
    public bool DisposeEventEntityAfterPersistence { get; set; }

    /// <summary>
    /// Returns the <see cref="IEventConverter"/> instance for the specified type.
    /// </summary>
    /// <param name="event">The event to convert.</param>
    /// <returns>The <see cref="IEventConverter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The converter was not 
    /// found.</exception>"
    public IEventConverter GetEventConverterFor(
        IEvent @event)
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
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(
                        type.GetTypeName()));

    /// <summary>
    /// Returns the <see cref="IEventFilter"/> instance for the specified 
    /// type.
    /// </summary>
    /// <param name="type">The event type to filter.</param>
    /// <returns>The <see cref="IEventFilter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The filter was not found.
    /// </exception>
    public IEventFilter GetEventFilterFor(
        Type type)
        => Filters
            .FirstOrDefault(x => x.CanFilter(type))
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindFilter
                    .StringFormat(type.GetTypeName()));

    /// <summary>
    /// Returns the <see cref="IEventFilter"/> instance for the specified 
    /// type.
    /// </summary>
    /// <typeparam name="TEvent">The type to filter.</typeparam>
    /// <returns>The <see cref="IEventFilter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The filter was not found.
    /// </exception>
    public IEventFilter GetEventFilterFor<TEvent>()
        where TEvent : class, IEvent
        => GetEventFilterFor(typeof(TEvent));

    /// <summary>
    /// Builds the default <see cref="EventOptions"/> instance.
    /// </summary>
    /// <param name="options">The <see cref="EventOptions"/> instance to 
    /// configure.</param>
    public static void Default(EventOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Converters.Add(new EventDomainConverter());
        options.Converters.Add(new EventIntegrationConverter());
        options.Converters.Add(new EventSnapshotConverter());

        options.Filters.Add(new EntityEventDomainFilter());
        options.Filters.Add(new EntityEventIntegrationFilter());
        options.Filters.Add(new EntityEventSnapshotFilter());

        options.DisposeEventEntityAfterPersistence = true;

        options.SerializerOptions ??= new JsonSerializerOptions(
            JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = true
        };
    }
}


/// <summary>
/// Defines the scheduler options.
/// </summary>
public sealed record class SchedulerOptions
{
    /// <summary>
    /// The delay between two executions.
    /// </summary>
    /// <remarks>The default value is 15000.</remarks>
    public int DelayMilliSeconds { get; init; } = 15000;

    /// <summary>
    /// The total number of integration events to load for each thread.
    /// </summary>
    /// <remarks>The default value is 100.</remarks>
    public int TotalPerThread { get; init; } = 100;

    /// <summary>
    /// The total number of attempts in case of exception.
    /// </summary>
    /// <remarks>The default value is 5.</remarks>
    public int MaxAttempts { get; init; } = 5;
}

/// <summary>
/// Determines the <see cref="SnapshotOptions"/> status.
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
public sealed record class SnapshotOptions
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
