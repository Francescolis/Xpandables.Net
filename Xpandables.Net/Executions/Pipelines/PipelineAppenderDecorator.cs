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
/// A pipeline decorator that appends dependency aggregates to the related store upon successful pipeline processing.
/// </summary>
/// <param name="serviceProvider">The service provider used to resolve required services.</param>
/// <typeparam name="TRequest">The type of the request, which must implement <see cref="IDependencyRequest"/> and
/// <see cref="IAggregateAppended"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response, which must inherit from <see cref="Result"/>.</typeparam>
public sealed class PipelineAppenderDecorator<TRequest, TResponse>(IServiceProvider serviceProvider) :
    IPipelineDecorator<TRequest, TResponse>
    where TRequest : class, IDependencyRequest, IAggregateAppended
    where TResponse : Result
{
    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        TResponse response = await next().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return response;
        }

        Type aggregateStoreType = typeof(IAggregateStore<>)
            .MakeGenericType(context.Request.DependencyType);

        IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
            .GetRequiredService(aggregateStoreType);

        await aggregateStore
            .AppendAsync((Aggregate)context.Request.DependencyInstance, cancellationToken)
            .ConfigureAwait(false);

        return response;
    }
}