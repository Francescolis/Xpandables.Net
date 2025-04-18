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

using Xpandables.Net.Executions.Deciders;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Decorator for handling <see cref="IDependencyRequest{TDependency}"/>> in a pipeline.
/// it provides a way to apply the decider pattern to the request object.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineDependencyDecorator<TRequest, TResponse>(
    IDependencyManager dependencyManager) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IDependencyRequest
    where TResponse : notnull
{
    /// <inheritdoc/>
    public override async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        IDependencyProvider dependencyProvider = dependencyManager
            .GetDependencyProvider(request.DependencyType);

        object dependency = await dependencyProvider
            .GetDependencyAsync(request, cancellationToken)
            .ConfigureAwait(false);

        request.DependencyInstance = dependency;

        return await next().ConfigureAwait(false);
    }
}
