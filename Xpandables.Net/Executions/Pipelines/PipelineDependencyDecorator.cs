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

using Xpandables.Net.Executions.Dependencies;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Implements a pipeline decorator that resolves and injects dependencies into the request during pipeline execution.
/// </summary>
/// <typeparam name="TRequest">The type of the request object, which must implement <see cref="IDependencyRequest"/>.</typeparam>
public sealed class PipelineDependencyDecorator<TRequest>(
    IDependencyManager dependencyManager) : IPipelineDecorator<TRequest>
    where TRequest : class, IDependencyRequest
{
    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken = default)
    {
        IDependencyProvider dependencyProvider = dependencyManager
            .GetDependencyProvider(context.Request.DependencyType);

        object dependency = await dependencyProvider
            .GetDependencyAsync(context.Request, cancellationToken)
            .ConfigureAwait(false);

        context.Request.DependencyInstance = dependency;

        return await next(cancellationToken).ConfigureAwait(false);
    }
}
