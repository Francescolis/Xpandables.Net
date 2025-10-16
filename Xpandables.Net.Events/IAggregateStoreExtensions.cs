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
/// Provides extension methods for working with implementations of the IAggregateStore interface.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IAggregateStoreExtensions
{
    /// <summary>
    /// Extension methods for <see cref="IAggregateStore"/>.
    /// </summary>
    extension(IAggregateStore aggregateStore)
    {
        /// <summary>
        /// Asynchronously loads an aggregate of the specified type from the underlying aggregate store using the
        /// provided stream identifier.
        /// </summary>
        /// <typeparam name="TAggregate">The type of aggregate to load. Must be a class that implements the IAggregate interface and has a
        /// parameterless constructor.</typeparam>
        /// <param name="streamId">The unique identifier of the aggregate stream to load.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the load operation.</param>
        /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded aggregate
        /// instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the aggregate store has not been initialized.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the aggregate store does not support the specified aggregate type.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public async Task<TAggregate> LoadAsync<TAggregate>(Guid streamId, CancellationToken cancellationToken = default)
            where TAggregate : class, IAggregate, new()
            => (aggregateStore ?? throw new ArgumentNullException(nameof(aggregateStore))) switch
            {
                IAggregateStore<TAggregate> typedStore => await typedStore.LoadAsync(streamId, cancellationToken).ConfigureAwait(false),
                _ => throw new InvalidOperationException($"The aggregate store must be of type '{typeof(IAggregateStore<TAggregate>)}'.")
            };

        /// <summary>
        /// Asynchronously saves the specified aggregate instance to the underlying aggregate store.
        /// </summary>
        /// <typeparam name="TAggregate">The type of aggregate to save. Must implement the IAggregate interface and have a parameterless constructor.</typeparam>
        /// <param name="aggregate">The aggregate instance to be saved. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the underlying aggregate store does not support the specified aggregate type.</exception>
        public async Task SaveAsync<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
            where TAggregate : class, IAggregate, new()
        {
            ArgumentNullException.ThrowIfNull(aggregateStore);
            if (aggregateStore is not IAggregateStore<TAggregate> typedStored)
                throw new InvalidOperationException($"The aggregate store must be of type '{typeof(IAggregateStore<TAggregate>)}'.");

            await typedStored.SaveAsync(aggregate, cancellationToken).ConfigureAwait(false);
        }
    }
}
