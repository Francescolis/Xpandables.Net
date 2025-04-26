using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Defines the contract for an event bus.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes the specified event to the event bus.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EventBus(IMessageQueue messageQueue) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent =>
        await messageQueue.EnqueueAsync(@event, cancellationToken).ConfigureAwait(false);
}