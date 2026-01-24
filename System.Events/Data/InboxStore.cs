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
using System.Events.Integration;

namespace System.Events.Data;

/// <summary>
/// EF Core implementation of <see cref="IInboxStore"/> enabling idempotent (exactly-once) consumption.
/// </summary>
/// <typeparam name="TEntityEventInbox">Inbox entity type.</typeparam>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="converterFactory">The event converter factory.</param>
public sealed class InboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntityEventInbox>(
    IUnitOfWork unitOfWork,
    IEventConverterFactory converterFactory) : IInboxStore
    where TEntityEventInbox : class, IEntityEventInbox
{
    private readonly IRepository<TEntityEventInbox> _inboxRepository = unitOfWork.GetRepository<IRepository<TEntityEventInbox>>();
    private readonly IEventConverterFactory _converterFactory = converterFactory;
    private readonly IEventConverter<TEntityEventInbox, IIntegrationEvent> _converter =
        converterFactory.GetInboxEventConverter<TEntityEventInbox>();

    /// <inheritdoc />
    public async Task<InboxReceiveResult> ReceiveAsync(
        IIntegrationEvent @event,
        string consumer,
        TimeSpan? visibilityTimeout = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);

        _inboxRepository.IsUnitOfWorkEnabled = false;

        var specification = QuerySpecification
            .For<TEntityEventInbox>()
            .Where(e => e.KeyId == @event.EventId && e.Consumer == consumer)
            .Select(e => new { e.Status, e.NextAttemptOn });

        var existing = await _inboxRepository
            .FetchFirstOrDefaultAsync(specification, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null && existing.Status == EntityStatus.PUBLISHED)
        {
            return new InboxReceiveResult(@event.EventId, EntityStatus.DUPLICATE);
        }

        if (existing is not null && existing.Status == EntityStatus.PROCESSING)
        {
            return new InboxReceiveResult(@event.EventId, EntityStatus.PROCESSING);
        }

        if (existing is not null && existing.Status == EntityStatus.ONERROR)
        {
            // allow retry only if lease expired
            if (existing.NextAttemptOn is not null && existing.NextAttemptOn > DateTime.UtcNow)
            {
                return new InboxReceiveResult(@event.EventId, EntityStatus.PROCESSING);
            }
        }

        TEntityEventInbox entity = _converter.ConvertEventToEntity(@event, _converterFactory.ConverterContext);
        entity.SetStatus(EntityStatus.PROCESSING);
        entity.Consumer = consumer;
        entity.ClaimId = Guid.NewGuid();
        entity.NextAttemptOn = DateTime.UtcNow.Add(visibilityTimeout ?? TimeSpan.FromMinutes(5));

        await _inboxRepository.AddAsync([entity], cancellationToken).ConfigureAwait(false);

        return new InboxReceiveResult(@event.EventId, EntityStatus.ACCEPTED);
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task CompleteAsync(
        CancellationToken cancellationToken,
        params CompletedInboxEvent[] events)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentOutOfRangeException.ThrowIfEqual(events.Length, 0);

        _inboxRepository.IsUnitOfWorkEnabled = false;

        var now = DateTime.UtcNow;

        foreach (var evt in events)
        {
            var specification = QuerySpecification
                .For<TEntityEventInbox>()
                .Where(e => e.KeyId == evt.EventId && e.Consumer == evt.Consumer)
                .Build();

            var updater = EntityUpdater
                .For<TEntityEventInbox>()
                .SetProperty(e => e.Status, EntityStatus.PUBLISHED.Value)
                .SetProperty(e => e.ErrorMessage, (string?)null)
                .SetProperty(e => e.AttemptCount, e => e.AttemptCount)
                .SetProperty(e => e.NextAttemptOn, (DateTime?)null)
                .SetProperty(e => e.ClaimId, (Guid?)null)
                .SetProperty(e => e.UpdatedOn, now);

            await _inboxRepository
                .UpdateAsync(specification, updater, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public async Task FailAsync(
        CancellationToken cancellationToken,
        params FailedInboxEvent[] failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentOutOfRangeException.ThrowIfEqual(failures.Length, 0);

        _inboxRepository.IsUnitOfWorkEnabled = false;

        var now = DateTime.UtcNow;

        try
        {
            foreach (var failure in failures)
            {
                var specification = QuerySpecification
                    .For<TEntityEventInbox>()
                    .Where(e => e.KeyId == failure.EventId && e.Consumer == failure.Consumer)
                    .Build();

                var updater = EntityUpdater
                    .For<TEntityEventInbox>()
                    .SetProperty(e => e.Status, EntityStatus.ONERROR.Value)
                    .SetProperty(e => e.ErrorMessage, failure.Error)
                    .SetProperty(e => e.AttemptCount, e => e.AttemptCount + 1)
                    .SetProperty(e => e.NextAttemptOn, e => now.AddSeconds(
                        e.AttemptCount < 1 ? 10 :
                        e.AttemptCount < 2 ? 20 :
                        e.AttemptCount < 3 ? 40 :
                        e.AttemptCount < 4 ? 80 :
                        e.AttemptCount < 5 ? 160 :
                        e.AttemptCount < 6 ? 320 :
                        600))
                    .SetProperty(e => e.ClaimId, (Guid?)null)
                    .SetProperty(e => e.UpdatedOn, now);

                await _inboxRepository
                    .UpdateAsync(specification, updater, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("An error occurred while failing inbox events.", exception);
        }
    }
}