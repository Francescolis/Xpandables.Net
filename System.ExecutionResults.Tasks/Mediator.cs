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
using System.ExecutionResults.Pipelines;

using Microsoft.Extensions.DependencyInjection;

namespace System.ExecutionResults.Tasks;

/// <summary>
/// Provides a sealed implementation of the IMediator interface that dispatches requests to their corresponding pipeline
/// handlers using dependency injection.
/// </summary>
/// <param name="provider">The service provider used to resolve pipeline request handlers for incoming requests.</param>
public sealed class Mediator(IServiceProvider provider) : IMediator
{
    /// <inheritdoc />
    [Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public async Task<OperationResult> SendAsync<TRequest>(
        TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            IPipelineRequestHandler<TRequest> handler =
                provider.GetRequiredService<IPipelineRequestHandler<TRequest>>();

            return await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationResultException)
        {
            throw;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Don't convert cancellation to ExecutionResult
            throw;
        }
        catch (Exception exception)
        {
            return exception.ToOperationResult();
        }
    }
}