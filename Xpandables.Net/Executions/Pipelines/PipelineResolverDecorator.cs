﻿/*******************************************************************************
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

using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Pipelines;
internal sealed class PipelineResolverDecorator<TRequest, TResponse>(
    IServiceProvider serviceProvider) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IDependencyRequest
    where TResponse : notnull
{
    public override async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        Type aggregateStoreType = typeof(IAggregateStore<>)
            .MakeGenericType(request.DependencyType);

        IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
            .GetRequiredService(aggregateStoreType);

        request.DependencyInstance = await aggregateStore
            .ResolveAsync((Guid)request.DependencyKeyId, cancellationToken)
            .ConfigureAwait(false);

        return await next().ConfigureAwait(false);
    }
}
