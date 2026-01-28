/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Entities;
using System.Events.Integration;

using Microsoft.Extensions.Logging;

namespace System.Events;

/// <summary>
/// Decorates integration event handlers with inbox idempotency checks.
/// </summary>
/// <typeparam name="TEvent">Integration event type.</typeparam>
/// <typeparam name="TEventHandler">Handler type that implements both <see cref="IEventHandler{TEvent}"/> and <see cref="IInboxConsumer"/>.</typeparam>
public sealed partial class InboxEventHandlerDecorator<TEvent, TEventHandler>(
    TEventHandler inner,
    IInboxStore inbox,
    ILogger<InboxEventHandlerDecorator<TEvent, TEventHandler>> logger) : IEventHandler<TEvent>
    where TEvent : class, IIntegrationEvent
    where TEventHandler : class, IEventHandler<TEvent>, IInboxConsumer
{
    private readonly TEventHandler _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly IInboxStore _inbox = inbox ?? throw new ArgumentNullException(nameof(inbox));
    private readonly ILogger<InboxEventHandlerDecorator<TEvent, TEventHandler>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Must catch all to record failure in inbox before rethrowing")]
    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var consumer = _inner.Consumer;

        if (string.IsNullOrWhiteSpace(consumer))
        {
            consumer = _inner.GetType().FullName ?? _inner.GetType().Name;
        }

        var receiveResult = await _inbox.ReceiveAsync(
            @event,
            consumer,
            visibilityTimeout: null,
            cancellationToken).ConfigureAwait(false);

        if (receiveResult.Status == EntityStatus.DUPLICATE ||
            receiveResult.Status == EntityStatus.PROCESSING)
        {
            LogInboxSkipped(_logger, @event.EventId, consumer, receiveResult.Status);
            return;
        }

        try
        {
            await _inner.HandleAsync(@event, cancellationToken).ConfigureAwait(false);

            await _inbox.CompleteAsync(
                cancellationToken,
                new CompletedInboxEvent(@event.EventId, consumer)).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            try
            {
                await _inbox.FailAsync(
                    cancellationToken,
                    new FailedInboxEvent(@event.EventId, consumer, exception.ToString())).ConfigureAwait(false);
            }
            catch (Exception failException)
            {
                LogInboxFailError(_logger, failException, @event.EventId, consumer);
            }

            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Inbox skipped handling for event {EventId} (consumer {Consumer}) with status {Status}")]
    private static partial void LogInboxSkipped(ILogger logger, Guid eventId, string consumer, EntityStatus status);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to mark inbox event {EventId} as failed for consumer {Consumer}")]
    private static partial void LogInboxFailError(ILogger logger, Exception exception, Guid eventId, string consumer);
}
