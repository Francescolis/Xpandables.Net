
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
using System.Threading.Channels;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Defines an interface for a message queue used in processing integration events.
/// </summary>
#pragma warning disable CA1711
public interface IMessageQueue
#pragma warning restore CA1711
{
    /// <summary>
    /// The channel used for message queuing.
    /// </summary>
    Channel<IIntegrationEvent> Channel { get; }

    /// <summary>
    /// Enqueues a message for processing.
    /// </summary>
    /// <param name="message"> The message to enqueue.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EnqueueAsync(IIntegrationEvent message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues messages for processing.
    /// </summary>
    /// <param name="capacity">The maximum number of messages to dequeue.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DequeueAsync(ushort capacity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides a bounded message queue implementation that facilitates the enqueuing and dequeuing
/// of integration events used within an event-driven system.
/// </summary>
#pragma warning disable CA1711
public sealed class MessageQueue(IOutboxStore outboxStore) : IMessageQueue
#pragma warning restore CA1711
{
    /// <inheritdoc />
    public Channel<IIntegrationEvent> Channel { get; } =
        System.Threading.Channels.Channel.CreateBounded<IIntegrationEvent>(new BoundedChannelOptions(100)
        {
            // Tune these to your scheduler model. Defaults keep the existing behavior.
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait
        });

    /// <inheritdoc />
    public async Task EnqueueAsync(
        IIntegrationEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        await outboxStore
            .EnqueueAsync(message, cancellationToken)
            .ConfigureAwait(false);
        // SaveChanges is deferred to UnitOfWork decorator.
    }

    /// <inheritdoc />
    public async Task DequeueAsync(ushort capacity, CancellationToken cancellationToken = default)
    {
        if (capacity == 0 || cancellationToken.IsCancellationRequested) return;

        var batchSize = Math.Max(1, (int)capacity);
        var pending = await outboxStore
            .ClaimPendingAsync(batchSize, leaseDuration: null, cancellationToken)
            .ConfigureAwait(false);

        if (pending.Count == 0) return;

        foreach (var @event in pending)
        {
            // Respect cancellation and backpressure
            cancellationToken.ThrowIfCancellationRequested();
            await Channel.Writer
                .WriteAsync(@event, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}