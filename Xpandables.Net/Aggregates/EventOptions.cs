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

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Defines the event configuration options.
/// </summary>
public sealed record EventOptions
{
    /// <summary>
    /// Gets the key to use to store events unit of work in the
    /// service collection.
    /// </summary>
    public const string UnitOfWorkKey = nameof(IAggregate);

    /// <summary>
    /// Gets the list of user-defined converters that were registered.
    /// </summary>
    public IList<EventConverter> Converters { get; }
        = new List<EventConverter>();

    /// <summary>
    /// Gets the list of user-defined filters that were registered.
    /// </summary>
    public IList<EventEntityFilter> Filters { get; }
        = new List<EventEntityFilter>();

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
    public SchedulerOptions NotificationOptions { get; set; } = new();

    /// <summary>
    /// Determines whether to consider no event handler as an error.
    /// </summary>
    public bool ConsiderNoDomainEventHandlerAsError { get; set; }

    /// <summary>
    /// Determines whether to consider no integration event handler as an error.
    /// </summary>
    public bool ConsiderNoEventIntegrationHandlerAsError { get; set; }

    /// <summary>
    /// Returns the <see cref="EventConverter"/> instance for the specified type.
    /// </summary>
    /// <typeparam name="TEventConverter">The type of the event converter
    /// .</typeparam>
    /// <param name="type">The type to convert.</param>
    /// <returns>The <see cref="EventConverter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The converter was not 
    /// found.</exception>"
    public TEventConverter GetEventConverterFor<TEventConverter>(
        Type type)
        where TEventConverter : EventConverter
        => Converters
            .FirstOrDefault(x => x.CanConvert(type))
            .As<TEventConverter>()
              ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindConverter
                    .StringFormat(
                        typeof(TEventConverter).GetNameWithoutGenericArity()));

    /// <summary>
    /// Returns the <see cref="EventEntityFilter"/> instance for the specified 
    /// type.
    /// </summary>
    /// <typeparam name="TEventEntityFilter">The type of the event entity filter.
    /// <paramref name="type"/>The type to filter.</typeparam>
    /// <returns>The <see cref="EventEntityFilter"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The filter was not found.
    /// </exception>
    public TEventEntityFilter GetEventEntityFilterFor<TEventEntityFilter>(
        Type type)
        where TEventEntityFilter : EventEntityFilter
        => Filters
            .FirstOrDefault(x => x.CanFilter(type))
            .As<TEventEntityFilter>()
            ?? throw new InvalidOperationException(
                I18nXpandables.AggregateFailedToFindFilter
                    .StringFormat(
                        typeof(TEventEntityFilter)
                            .GetNameWithoutGenericArity()));

    /// <summary>
    /// Builds the default <see cref="EventOptions"/> instance.
    /// </summary>
    /// <param name="options">The <see cref="EventOptions"/> instance to 
    /// configure.</param>
    public static void Default(EventOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Converters.Add(new EventEntityDomainConverter());
        options.Converters.Add(new EventEntityNotificationConverter());
        options.Converters.Add(new EventEntitySnapshotConverter());
        options.Converters.Add(new AggregateEventConverter());

        options.Filters.Add(new EventEntityDomainFilter());
        options.Filters.Add(new EventEntityNotificationFilter());

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
/// Defines the <see cref="IEventIntegrationScheduler"/> options.
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
