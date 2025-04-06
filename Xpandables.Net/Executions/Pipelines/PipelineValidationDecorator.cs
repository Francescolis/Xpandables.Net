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
using System.Runtime.CompilerServices;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// A decorator that validates the request before passing it to the next 
/// delegate in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="validators">The composite validator instance.</param>
public sealed class PipelineValidationDecorator<TRequest, TResponse>(
    ICompositeValidator<TRequest> validators) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IValidationEnabled
    where TResponse : notnull
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest query,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        ExecutionResult result = await validators
            .ValidateAsync(query)
            .ConfigureAwait(false);

        if (result.IsFailureStatusCode())
        {
            throw new ExecutionResultException(result);
        }

        return await next().ConfigureAwait(false);
    }
}

/// <summary>
/// A decorator that validates the request before passing it to the next
/// next delegate in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="validators">The composite validator instance.</param>
public sealed class PipelineStreamValidationDecorator<TRequest, TResponse>(
    ICompositeValidator<TRequest> validators) :
    PipelineStreamDecorator<TRequest, TResponse>
    where TRequest : class, IValidationEnabled
    where TResponse : notnull
{
    /// <inheritdoc/>
    protected override async IAsyncEnumerable<TResponse> HandleCoreAsync(
        TRequest query,
        RequestStreamHandler<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ExecutionResult result = await validators
            .ValidateAsync(query)
            .ConfigureAwait(false);

        if (result.IsFailureStatusCode())
        {
            throw new ExecutionResultException(result);
        }

        await foreach (var item in next().ConfigureAwait(false))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return item;
        }
    }
}