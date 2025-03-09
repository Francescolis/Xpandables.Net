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

using Xpandables.Net.Executions.Deciders;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Decorator for handling <see cref="IDeciderRequest{TDependency}"/>> in a pipeline.
/// it provides a way to apply the decider pattern to the request object.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineDeciderDecorator<TRequest, TResponse>(
    IDeciderDependencyManager dependencyManager) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IDeciderRequest
    where TResponse : notnull
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IDeciderDependencyProvider dependencyProvider = dependencyManager
                .GetDependencyProvider(request.Type);

            object dependency = await dependencyProvider
                .GetDependencyAsync(request, cancellationToken)
                .ConfigureAwait(false);

            request.Dependency = dependency;

            TResponse response = await next().ConfigureAwait(false);

            return response;
        }
        catch (Exception exception)
            when (exception is not ValidationException
                and not InvalidOperationException
                and not UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"An error occurred getting dependency of the object " +
                $"with the key '{request.KeyId}'.",
                exception);
        }
    }
}
