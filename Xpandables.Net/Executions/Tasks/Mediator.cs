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

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Represents a mediator implementation that facilitates the sending of requests
/// and the handling of execution results in a pipeline-based pattern.
/// </summary>
/// <remarks>
/// This class retrieves an appropriate pipeline request handler for the given request type
/// and invokes its processing logic. Any exceptions not derived from
/// <see cref="ExecutionResultException"/> are converted into an <see cref="ExecutionResult"/>.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Mediator(IServiceProvider provider) : IMediator
{
    /// <inheritdoc />
    public async Task<ExecutionResult> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest
    {
        try
        {
            IPipelineRequestHandler<TRequest> handler =
                provider.GetRequiredService<IPipelineRequestHandler<TRequest>>();

            return await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }
}