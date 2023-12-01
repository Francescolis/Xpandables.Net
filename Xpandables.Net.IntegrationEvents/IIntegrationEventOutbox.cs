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
/// Used for an implementation of the Outbox pattern to persist 
/// <see cref="IIntegrationEvent"/> to be processed outside of the transaction.
/// </summary>
public interface IIntegrationEventOutbox : IDisposable
{
    /// <summary>
    /// Asynchronously appends all the <see cref="IIntegrationEvent"/> 
    /// from the current execution flow.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A value that represents an <see cref="OperationResult"/>.</returns>
    ValueTask<OperationResult> AppendAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously appends the specified <see cref="IIntegrationEvent"/>.
    /// </summary>
    /// <param name="event">The integration event to be used.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A value that represents an <see cref="OperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="event"/> is null.</exception>
    ValueTask<OperationResult> AppendAsync(
        IIntegrationEvent @event,
        CancellationToken cancellationToken = default);
}