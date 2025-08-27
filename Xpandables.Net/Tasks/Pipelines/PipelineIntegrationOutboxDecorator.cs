
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
using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Events;
using Xpandables.Net.Executions;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Tasks.Pipelines;

/// <summary>
/// Enqueues pending integration events into the outbox after the request handler runs.
/// Intended to run before UnitOfWork decorators so that enqueues are flushed together.
/// </summary>
public sealed class PipelineIntegrationOutboxDecorator<TRequest>(
    IPendingIntegrationEvents pending,
    IOutboxStore outbox) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IRequiresEventStorage
{
    private readonly IPendingIntegrationEvents _pending = pending;
    private readonly IOutboxStore _outbox = outbox;

    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken = default)
    {
        var result = await next(cancellationToken).ConfigureAwait(false);

        if (result.IsSuccessStatusCode)
        {
            foreach (var @event in _pending.Drain())
                await _outbox.EnqueueAsync(@event, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }
}