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
using Xpandables.Net.Executions;

namespace Xpandables.Net.Pipelines;

/// <summary>
/// Decorator that finalizes the execution result for requests that use a finalizer.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineFinalizerDecorator<TRequest, TResponse>(
    IExecutionResultFinalizer finalizer) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IApplyFinalizer
    where TResponse : IExecutionResult
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            TResponse response = await next().ConfigureAwait(false);

            if (finalizer.Finalize is not null)
            {
                response = MatchResponse(finalizer.Finalize(response));
            }

            return response;
        }
        catch (Exception exception)
            when (finalizer.CallFinalizeOnException)
        {
            return MatchResponse(
                finalizer.Finalize(exception.ToExecutionResult()));
        }
    }
}
