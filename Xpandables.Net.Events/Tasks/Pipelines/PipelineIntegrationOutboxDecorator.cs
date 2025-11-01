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
using Xpandables.Net.Events;
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Requests;
using Xpandables.Net.Requests.Pipelines;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.Tasks.Pipelines;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides a pipeline decorator that ensures integration events generated during request processing are reliably
/// stored in an outbox for later dispatch.
/// </summary>
/// <remarks>This decorator should be used in scenarios where integration events must be reliably captured and
/// stored as part of the request pipeline, such as in event-driven or distributed systems. Events are drained from the
/// buffer and enqueued to the outbox only if the request is processed successfully.</remarks>
/// <typeparam name="TRequest">The type of request being processed. Must implement <see cref="IRequest"/> and <see cref="IRequiresEventStorage"/>.</typeparam>
/// <param name="pending">The buffer that holds pending integration events generated during request execution.</param>
/// <param name="outbox">The outbox store used to persist integration events for reliable delivery.</param>
public sealed class PipelineIntegrationOutboxDecorator<TRequest>(
    IPendingIntegrationEventsBuffer pending,
    IOutboxStore outbox) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IRequiresEventStorage
{
    private readonly IPendingIntegrationEventsBuffer _pending = pending;
    private readonly IOutboxStore _outbox = outbox;

    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextHandler);

        var result = await nextHandler(cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            foreach (var @event in _pending.Drain())
                await _outbox.EnqueueAsync(cancellationToken, @event).ConfigureAwait(false);
        }

        return result;
    }
}