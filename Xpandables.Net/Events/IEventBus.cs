namespace Xpandables.Net.Events;

/// <summary>
/// Provides an interface for an event bus that facilitates the publishing of events
/// within an event-driven architecture.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes the specified event to the associated handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to be published.</typeparam>
    /// <param name="event">The event instance to publish.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class EventBus(IMessageQueue messageQueue) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent =>
        await messageQueue.EnqueueAsync(@event, cancellationToken).ConfigureAwait(false);
}