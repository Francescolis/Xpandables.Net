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

using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;
using Xpandables.Net.Optionals;

namespace Xpandables.Net.Responsibilities.Decorators;

/// <summary>
/// Decorator for handling command aggregates in a pipeline using the
/// Decider pattern.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class AggregatePipelineDecorator<TRequest, TResponse>(
    IServiceProvider provider) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, ICommandAggregate
    where TResponse : IOperationResult
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Type aggregateStoreType = typeof(IAggregateStore<>)
                .MakeGenericType(request.AggregateType);

            dynamic aggregateStore = provider.GetRequiredService(aggregateStoreType);

            IAggregate aggregate = await aggregateStore
                .PeekAsync(request.KeyId, cancellationToken)
                .ConfigureAwait(false);

            request.Aggregate = aggregate.ToOptional();

            TResponse result = await next().ConfigureAwait(false);

            if (result.IsSuccessStatusCode)
            {
                await aggregateStore
                    .AppendAsync(aggregate, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception exception)
            when (exception is not ValidationException and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"The aggregate with the key '{request.KeyId}' was not found.",
                exception);
        }
    }
}
