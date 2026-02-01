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
using System.Diagnostics.CodeAnalysis;
using System.Entities;
using System.Entities.Data;
using System.Events.Integration;

namespace System.Events.Data;

/// <summary>
/// ADO.NET implementation of <see cref="IInboxStore"/> enabling idempotent (exactly-once) consumption.
/// </summary>
/// <typeparam name="TEntityEventInbox">Inbox entity type.</typeparam>
/// <remarks>
/// <para>
/// This class implements the inbox pattern using raw ADO.NET (not Entity Framework Core),
/// providing idempotent event processing with exactly-once semantics.
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class InboxStore<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntityEventInbox> : IInboxStore
    where TEntityEventInbox : class, IEntityEventInbox
{
    private readonly IDataRepository<TEntityEventInbox> _inboxRepository;
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly IEventConverterFactory _converterFactory;
    private readonly IEventConverter<TEntityEventInbox, IIntegrationEvent> _converter;

    /// <summary>
    /// Initializes a new instance of the DataInboxStore class.
    /// </summary>
    /// <param name="unitOfWork">The ADO.NET unit of work.</param>
    /// <param name="converterFactory">The event converter factory.</param>
    public InboxStore(
        IDataUnitOfWork unitOfWork,
        IEventConverterFactory converterFactory)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(converterFactory);

        _unitOfWork = unitOfWork;
        _inboxRepository = unitOfWork.GetRepository<TEntityEventInbox>();
        _converterFactory = converterFactory;
        _converter = converterFactory.GetInboxEventConverter<TEntityEventInbox>();
    }

    /// <inheritdoc />
    public async Task<InboxReceiveResult> ReceiveAsync(
        IIntegrationEvent @event,
        string consumer,
        TimeSpan? visibilityTimeout = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);

        // Check for existing entry
        var specification = QuerySpecification
            .For<TEntityEventInbox>()
            .Where(e => e.KeyId == @event.EventId && e.Consumer == consumer)
            .Build();

        var existing = await _inboxRepository
            .QueryFirstOrDefaultAsync(specification, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            if (existing.Status == EventStatus.PUBLISHED.Value)
            {
                return new InboxReceiveResult(@event.EventId, EventStatus.DUPLICATE);
            }

            if (existing.Status == EventStatus.PROCESSING.Value)
            {
                return new InboxReceiveResult(@event.EventId, EventStatus.PROCESSING);
            }

            if (existing.Status == EventStatus.ONERROR.Value)
            {
                // Allow retry only if lease expired
                if (existing.NextAttemptOn is not null && existing.NextAttemptOn > DateTime.UtcNow)
                {
                    return new InboxReceiveResult(@event.EventId, EventStatus.PROCESSING);
                }
            }
        }

        // Insert new inbox entry
        TEntityEventInbox entity = _converter.ConvertEventToEntity(@event, _converterFactory.ConverterContext);
        entity.SetStatus(EventStatus.PROCESSING.Value);
        entity.Consumer = consumer;
        entity.ClaimId = Guid.NewGuid();
        entity.NextAttemptOn = DateTime.UtcNow.Add(visibilityTimeout ?? TimeSpan.FromMinutes(5));

        await _inboxRepository.InsertAsync(entity, cancellationToken).ConfigureAwait(false);

        return new InboxReceiveResult(@event.EventId, EventStatus.ACCEPTED);
    }

    /// <inheritdoc />
    public async Task CompleteAsync(
        CancellationToken cancellationToken,
        params CompletedInboxEvent[] events)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfEqual(events.Length, 0);

        var now = DateTime.UtcNow;

        foreach (var evt in events)
        {
            var specification = QuerySpecification
                .For<TEntityEventInbox>()
                .Where(e => e.KeyId == evt.EventId && e.Consumer == evt.Consumer)
                .Build();

            var updater = EntityUpdater
                .For<TEntityEventInbox>()
                .SetProperty(e => e.Status, EventStatus.PUBLISHED.Value)
                .SetProperty(e => e.ErrorMessage, (string?)null)
                .SetProperty(e => e.NextAttemptOn, (DateTime?)null)
                .SetProperty(e => e.ClaimId, (Guid?)null)
                .SetProperty(e => e.UpdatedOn, now);

            await _inboxRepository
                .UpdateAsync(specification, updater, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task FailAsync(
        CancellationToken cancellationToken,
        params FailedInboxEvent[] failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentOutOfRangeException.ThrowIfEqual(failures.Length, 0);

        var now = DateTime.UtcNow;

        foreach (var failure in failures)
        {
            // Get current attempt count
            var getSpec = QuerySpecification
                .For<TEntityEventInbox>()
                .Where(e => e.KeyId == failure.EventId && e.Consumer == failure.Consumer)
                .Select(e => e.AttemptCount);

            var currentAttempt = await _inboxRepository
                .QueryFirstOrDefaultAsync(getSpec, cancellationToken)
                .ConfigureAwait(false);

            var nextAttempt = currentAttempt + 1;
            var backoffSeconds = nextAttempt < 1 ? 10 :
                                 nextAttempt < 2 ? 20 :
                                 nextAttempt < 3 ? 40 :
                                 nextAttempt < 4 ? 80 :
                                 nextAttempt < 5 ? 160 : 320;

            var specification = QuerySpecification
                .For<TEntityEventInbox>()
                .Where(e => e.KeyId == failure.EventId && e.Consumer == failure.Consumer)
                .Build();

            var updater = EntityUpdater
                .For<TEntityEventInbox>()
                .SetProperty(e => e.Status, EventStatus.ONERROR.Value)
                .SetProperty(e => e.ErrorMessage, failure.Error)
                .SetProperty(e => e.AttemptCount, nextAttempt)
                .SetProperty(e => e.NextAttemptOn, now.AddSeconds(backoffSeconds))
                .SetProperty(e => e.ClaimId, (Guid?)null)
                .SetProperty(e => e.UpdatedOn, now);

            await _inboxRepository
                .UpdateAsync(specification, updater, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
