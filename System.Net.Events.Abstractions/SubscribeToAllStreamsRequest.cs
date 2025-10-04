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
namespace System.Net.Events;

/// <summary>
/// Represents a request to subscribe to all event streams, starting from a specified global position.
/// </summary>
/// <remarks>Use this type to initiate a subscription that receives events from all streams, beginning at the
/// given position. The subscription will invoke the provided callback for each event received. This request is
/// typically used in event sourcing or message-driven systems to process or react to all events in the
/// system.</remarks>
public readonly record struct SubscribeToAllStreamsRequest
{
    /// <summary>
    /// Initializes a new instance of the SubscribeToAllRequest class.
    /// </summary>
    public SubscribeToAllStreamsRequest()
    {
    }

    /// <summary>
    /// Gets the starting position to subscribe from (inclusive).
    /// </summary>
    /// <remarks>If the specified position is less than the current global position, historical events
    /// will be replayed from that position onward. If it is equal to the current position,
    /// events will be received in real-time as they are appended to any stream.</remarks>
    public readonly required long FromPosition { get; init; } = 0;

    /// <summary>
    /// Gets the callback function to invoke when an event is received.
    /// </summary>
    public readonly required Func<IDomainEvent, ValueTask> OnEvent { get; init; }
}