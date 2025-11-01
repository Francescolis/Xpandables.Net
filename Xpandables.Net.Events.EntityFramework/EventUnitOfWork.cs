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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Xpandables.Net.Events.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides a unit of work implementation that coordinates transactional changes across both the event store and outbox
/// store, ensuring atomic persistence of events and outbox messages.
/// </summary>
/// <remarks>This class ensures that changes to the event store and outbox store are committed within a single
/// transaction, maintaining consistency between event persistence and outbox message dispatch. It is intended for
/// scenarios where reliable event publishing and transactional integrity are required.</remarks>
/// <param name="eventStoreData">The event store data context used to manage and persist domain events within the unit of work.</param>
/// <param name="outboxStoreData">The outbox store data context used to manage and persist outbox messages for reliable event delivery.</param>
public sealed class EventUnitOfWork(
    EventStoreDataContext eventStoreData,
    OutboxStoreDataContext outboxStoreData) : IEventUnitOfWork
{
    /// <summary>
    /// Asynchronously saves all changes made in the context to the underlying database and outbox store within a single
    /// transaction.
    /// </summary>
    /// <remarks>All changes are persisted using a database transaction to ensure atomicity between the main
    /// data store and the outbox store. If the operation is canceled or an error occurs, the transaction is rolled back
    /// and no changes are committed.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries
    /// written to the database.</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // persist using transaction

        IExecutionStrategy strategy = eventStoreData.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await eventStoreData.Database
                    .BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    var result = await eventStoreData
                        .SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);

                    await outboxStoreData
                        .SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);

                    await transaction
                        .CommitAsync(cancellationToken)
                        .ConfigureAwait(false);

                    return result;
                }
                catch
                {
                    await transaction
                        .RollbackAsync(cancellationToken)
                        .ConfigureAwait(false);

                    throw;
                }
            })
            .ConfigureAwait(false);

    }
}
