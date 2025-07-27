using System.Threading.Channels;

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Executions.Tasks;

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

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Provides a bounded message queue implementation that facilitates the enqueuing and dequeuing
/// of integration events used within an event-driven system.
/// </summary>
#pragma warning disable CA1711
public sealed class MessageQueue(IEventStore eventStore) : IMessageQueue
#pragma warning restore CA1711
{
    /// <inheritdoc />
    public Channel<IIntegrationEvent> Channel { get; } =
        System.Threading.Channels.Channel.CreateBounded<IIntegrationEvent>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait
        });

    /// <inheritdoc />
    public async Task EnqueueAsync(
        IIntegrationEvent message, CancellationToken cancellationToken = default) =>
        await eventStore
            .AppendAsync(message, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task DequeueAsync(ushort capacity, CancellationToken cancellationToken = default)
    {
        Func<IQueryable<EntityIntegrationEvent>, IQueryable<EntityIntegrationEvent>> filter = query =>
            query.Where(w => w.Status == EntityStatus.PENDING.Value)
                .OrderBy(o => o.CreatedOn)
                .Take(capacity);

        await foreach (IEvent @event in eventStore
            .FetchAsync(filter, cancellationToken)
            .AsEventsAsync(cancellationToken)
            .WithCancellation(cancellationToken))
        {
            await Channel.Writer
                .WriteAsync((IIntegrationEvent)@event, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}