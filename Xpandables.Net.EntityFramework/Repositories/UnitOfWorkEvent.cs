
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
using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a unit of work event that manages the lifecycle of a data context and associated event stores.
/// </summary>
/// <remarks>This class is responsible for coordinating the saving of changes to the data context and managing
/// event stores that are used within the unit of work. It ensures that all resources are properly disposed of when the
/// unit of work is completed. The class is sealed to prevent inheritance.</remarks>
/// <param name="context">The data context event associated with this unit of work.</param>
/// <param name="serviceProvider">The service provider used to resolve event store dependencies.</param>
public sealed class UnitOfWorkEvent(DataContextEvent context, IServiceProvider serviceProvider) :
    AsyncDisposable, IUnitOfWorkEvent<DataContextEvent>
{
    private readonly ConcurrentDictionary<Type, IEventStore> _eventStores = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly DataContextEvent _context = context;

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while saving the changes.",
                exception);
        }
    }

    /// <inheritdoc />
    public IEventStore GetEventStore<TEventStore>()
        where TEventStore : class, IEventStore
    {
        var repository = _eventStores.GetOrAdd(typeof(TEventStore), _ =>
        {
            var service = _serviceProvider.GetService<TEventStore>()
                ?? throw new InvalidOperationException(
                    $"The event store of type {typeof(TEventStore).Name} is not registered.");

            // Inject the ambient DataContext into the event store
            DataContextExtensions.InjectAmbientContext(service, _context);

            return service;
        });

        return (TEventStore)repository;
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await _context.DisposeAsync().ConfigureAwait(false);
            foreach (var eventStore in _eventStores.Values)
            {
                await eventStore.DisposeAsync().ConfigureAwait(false);
            }
            _eventStores.Clear();
        }

        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
}