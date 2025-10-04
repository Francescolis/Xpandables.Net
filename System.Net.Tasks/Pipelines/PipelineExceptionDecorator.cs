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
using System.Net.ExecutionResults;
using System.Net.Tasks;

namespace System.Net.Tasks.Pipelines;

/// <summary>
/// A pipeline decorator that handles exceptions thrown during the execution 
/// of a request and transforms them into an <see cref="ExecutionResult"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public sealed class PipelineExceptionDecorator<TRequest>(
    IRequestExceptionHandler<TRequest>? exceptionHandler = default) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextHandler);

        try
        {
            return await nextHandler(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not ExecutionResultException)
        {
            if (exceptionHandler is not null)
            {
                try
                {
                    return await exceptionHandler
                        .HandleAsync(context, exception, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not ExecutionResultException)
                {
                    AggregateException aggregateException = new(
                        "An error occurred while handling the exception.",
                        ex,
                        exception);

                    return aggregateException.ToExecutionResult();
                }
            }

            return exception.ToExecutionResult();
        }
    }
}
