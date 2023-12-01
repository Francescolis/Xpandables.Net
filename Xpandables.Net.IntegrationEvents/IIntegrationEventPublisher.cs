/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using Xpandables.Net.Operations;

namespace Xpandables.Net.IntegrationEvents;

/// <summary>
/// Defines a method to automatically publish integration events.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes the specified event to all registered subscribers.
    /// </summary>
    /// <typeparam name="TIntegrationEvent">Type of event.</typeparam>
    /// <param name="event">The event to be published.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents an asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
    /// <returns>A value that represents an implementation of <see cref="IOperationResult"/>.</returns>
    ValueTask<OperationResult> PublishAsync<TIntegrationEvent>(
        TIntegrationEvent @event,
        CancellationToken cancellationToken = default)
        where TIntegrationEvent : notnull, IIntegrationEvent;
}