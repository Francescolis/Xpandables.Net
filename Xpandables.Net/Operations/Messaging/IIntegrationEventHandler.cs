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
namespace Xpandables.Net.Operations.Messaging;

/// <summary>
/// Represents a method signature to be used to apply 
/// <see cref="IIntegrationEventHandler{TMessage}"/> implementation.
/// </summary>
/// <typeparam name="T">The integration event type.</typeparam>
/// <param name="event">The integration event instance to act on.</param>
/// <param name="cancellationToken">A CancellationToken to observe while 
/// waiting for the task to complete.</param>
/// <returns>A value that represents an implementation of <see cref="IOperationResult"/>.</returns>
public delegate ValueTask<OperationResult> IntegrationEventHandler<in T>(
    T @event, CancellationToken cancellationToken = default)
    where T : notnull;

/// <summary>
/// Allows an application author to define a handler for specific type integration event.
/// The integration event must implement <see cref="IIntegrationEvent"/> interface.
/// The implementation must be thread-safe when working in a multi-threaded environment.
/// </summary>
/// <typeparam name="TIntegrationEvent">The integration event type to be handled.</typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : notnull, IIntegrationEvent
{
    /// <summary>
    /// Asynchronously handles the integration event of specific type.
    /// </summary>
    /// <param name="event">The integration event instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
    /// <returns>A value that represents an <see cref="OperationResult"/>.</returns>
    ValueTask<OperationResult> HandleAsync(
        TIntegrationEvent @event, CancellationToken cancellationToken = default);
}
