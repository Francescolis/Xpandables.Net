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

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// A decorator implementation for pipeline processing of a specified request and response.
/// This class resolves a dependency instance from a store based on the request's key,
/// and enriches the request with the resolved dependency before proceeding with the pipeline.
/// </summary>
/// <param name="serviceProvider">The service provider used to resolve dependencies required by the pipeline.</param>
/// <typeparam name="TRequest">The type of the request that implements IDependencyRequest.</typeparam>
/// <typeparam name="TResponse">The type of the response that inherits from _ExecutionResult.</typeparam>
public sealed class PipelineResolverDecorator<TRequest, TResponse>(IServiceProvider serviceProvider) :
    IPipelineDecorator<TRequest, TResponse>
    where TRequest : class, IDependencyRequest, IAggregateResolved
    where TResponse : Result
{
    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        Type aggregateStoreType = typeof(IAggregateStore<>)
            .MakeGenericType(context.Request.DependencyType);

        IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
            .GetRequiredService(aggregateStoreType);

        context.Request.DependencyInstance = await aggregateStore
            .ResolveAsync((Guid)context.Request.DependencyKeyId, cancellationToken)
            .ConfigureAwait(false);

        return await next().ConfigureAwait(false);
    }
}
