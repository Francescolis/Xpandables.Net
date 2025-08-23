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
namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a unit of work event that manages the lifecycle of a data context and associated event stores.
/// </summary>
/// <remarks>This class is responsible for coordinating the saving of changes to the data context and managing
/// event stores that are used within the unit of work. It ensures that all resources are properly disposed of when the
/// unit of work is completed. The class is sealed to prevent inheritance.</remarks>
/// <param name="context">The data context event associated with this unit of work.</param>
/// <param name="serviceProvider">The service provider used to resolve event store dependencies.</param>
public class UnitOfWorkEvent(DataContext context, IServiceProvider serviceProvider) :
    UnitOfWork(context, serviceProvider), IUnitOfWorkEvent
{
    /// <inheritdoc />
    public IEventStore GetEventStore<TEventStore>()
        where TEventStore : class, IEventStore =>
        GetRepository<TEventStore>();
}

/// <summary>
/// Represents a unit of work for handling events within a specified data context.
/// </summary>
/// <typeparam name="TDataContext">The type of the data context used by this unit of work. 
/// Must inherit from <see cref="DataContext"/>.</typeparam>
/// <param name="context">The data context to be used for this unit of work.</param>
/// <param name="serviceProvider">The service provider to resolve dependencies.</param>
public class UnitOfWorkEvent<TDataContext>(
    TDataContext context,
    IServiceProvider serviceProvider) : UnitOfWorkEvent(context, serviceProvider)
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the data context associated with this unit of work.
    /// </summary>
    protected new TDataContext Context { get; } = context;
}