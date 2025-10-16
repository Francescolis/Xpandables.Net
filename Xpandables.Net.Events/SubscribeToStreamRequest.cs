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
namespace Xpandables.Net.Events;

/// <summary>
/// Represents a request to subscribe to an event stream, specifying the stream identifier, starting version, and event
/// callback.
/// </summary>
/// <remarks>Use this type to initiate a subscription to a specific event stream. The subscription will begin from
/// the specified version, replaying historical events if necessary, and will invoke the provided callback for each
/// event received.</remarks>
public readonly record struct SubscribeToStreamRequest
{
    /// <summary>
    /// Initializes a new instance of the SubscribeToStreamRequest class.
    /// </summary>
    public SubscribeToStreamRequest()
    {
    }

    /// <summary>
    /// Gets the identifier of the event stream to subscribe to.
    /// </summary>
    public readonly required Guid StreamId { get; init; }

    /// <summary>
    /// Gets the starting version to subscribe from (inclusive).
    /// </summary>
    /// <remarks>If the specified version is less than the current stream version, historical events
    /// will be replayed from that version onward. If it is equal to the current version,
    /// events will be received in real-time as they are appended to the stream.</remarks>
    public readonly required long FromVersion { get; init; } = 0;

    /// <summary>
    /// Gets the interval at which polling operations are performed.
    /// </summary>
    /// <remarks>The default value is set to 1 second.</remarks> 
    public readonly TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets the callback function to invoke when an event is received.
    /// </summary>
    public readonly required Func<IDomainEvent, ValueTask> OnEvent { get; init; }
}
