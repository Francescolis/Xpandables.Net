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
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Pipelines;
internal sealed class PipelineAggregateDecorator<TRequest, TResponse>(
    IServiceProvider serviceProvider) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IDeciderRequest, IAggregateAppender
    where TResponse : class
{
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        bool appenderIsStarted = false;
        try
        {
            TResponse response = await next().ConfigureAwait(false);

            appenderIsStarted = true;

            await AggregateAppendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return response;
        }
        catch (Exception exception)
        {
            if (!appenderIsStarted)
            {
                try
                {
                    await AggregateAppendAsync(request, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception appenderException)
                    when (exception is not ValidationException
                        and not InvalidOperationException
                        and not UnauthorizedAccessException)
                {
                    throw new InvalidOperationException(
                        $"An error occurred when appending aggregate " +
                        $"with the key '{request.KeyId}'.",
                        new AggregateException(exception, appenderException));
                }
            }

            if (exception is not ValidationException
                and not InvalidOperationException
                and not UnauthorizedAccessException)
            {
                throw new InvalidOperationException(
                    $"An error occurred when appending aggregate " +
                    $"with the key '{request.KeyId}'.",
                    exception);
            }

            throw;
        }

        async Task AggregateAppendAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (request.Dependency is not null)
            {
                Type aggregateStoreType = typeof(IAggregateStore<>)
                    .MakeGenericType(request.Type);

                IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
                    .GetRequiredService(aggregateStoreType);

                await aggregateStore
                    .AppendAsync((IAggregate)request.Dependency, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
