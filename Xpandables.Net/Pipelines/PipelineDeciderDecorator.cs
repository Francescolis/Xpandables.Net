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

using Xpandables.Net.Commands;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Pipelines;

/// <summary>
/// Decorator for handling <see cref="ICommandDecider{TDependency}"/>> in a pipeline.
/// it provides a way to apply the decider pattern to the command object.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineDeciderDecorator<TRequest, TResponse>(
    ICommandDeciderDependencyProvider dependencyProvider) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, ICommandDecider
    where TResponse : IExecutionResult
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            object dependency = await dependencyProvider
                .GetDependencyAsync(request, cancellationToken)
                .ConfigureAwait(false);

            request.Dependency = dependency;

            TResponse result = await next().ConfigureAwait(false);

            return result;
        }
        catch (Exception exception)
            when (exception is not ValidationException and not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"An error occurred when applying decider pattern to the object " +
                $"with the key '{request.KeyId}'.",
                exception);
        }
    }
}
