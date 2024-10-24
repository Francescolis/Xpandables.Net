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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;

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
    private static readonly MethodInfo _doGetAggregate =
        typeof(AggregatePipelineDecorator<,>)
        .GetMethod(
            nameof(GetAggregate),
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <inheritdoc/>
    protected override Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        MethodInfo doBuildAsyncInvokable = _doGetAggregate
            .MakeGenericMethod(request.AggregateType);

        return (Task<TResponse>)doBuildAsyncInvokable
            .Invoke(null, [provider, request, next, cancellationToken])!;
    }

    private static async Task<TResponse> GetAggregate<TAggregate>(
        IServiceProvider provider,
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IAggregate, new()
    {
        IAggregateStore<TAggregate> aggregateStore =
            provider.GetRequiredService<IAggregateStore<TAggregate>>();

        try
        {
            TAggregate aggregate = await aggregateStore
                .PeekAsync(request.KeyId, cancellationToken)
                .ConfigureAwait(false);

            request.Aggregate = aggregate;
        }
        catch (ValidationException exception)
            when (!request.ContinueWhenNotFound)
        {
            throw new InvalidOperationException(
                $"The aggregate with the key '{request.KeyId}' was not found.",
                exception);
        }

        TResponse result = await next().ConfigureAwait(false);

        return result;
    }
}
