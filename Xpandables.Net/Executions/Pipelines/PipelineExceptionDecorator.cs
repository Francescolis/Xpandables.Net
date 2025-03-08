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
namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// A pipeline decorator that handles exceptions thrown during the execution 
/// of a request.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineExceptionDecorator<TRequest, TResponse> :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <inheritdoc/>
    protected override TResponse HandleCore(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return next();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            throw new ExecutionResultException(exception.ToExecutionResult());
        }
    }
}
