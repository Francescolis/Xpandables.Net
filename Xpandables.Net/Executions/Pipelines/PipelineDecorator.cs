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
namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Represents an abstract base class for pipeline decorators.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract class PipelineDecorator<TRequest, TResponse> :
    IPipelineDecorator<TRequest, TResponse>
    where TRequest : class
    where TResponse : IExecutionResult
{
    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await HandleCoreAsync(request, next, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ExecutionResultException executionException)
        {
            return MatchResponse(executionException.ExecutionResult);
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return MatchResponse(exception.ToExecutionResult());
        }
    }

    /// <summary>
    /// Matches the provided operation result to the expected response type.
    /// </summary>
    /// <param name="executionResult">The operation result to match.</param>
    /// <returns>The matched response of type TResponse.</returns>
    protected TResponse MatchResponse(IExecutionResult executionResult)
    {
        if (typeof(TResponse).IsGenericType)
        {
            Type resultType = typeof(TResponse).GetGenericArguments()[0];
            return (TResponse)executionResult.ToExecutionResult(resultType);
        }

        return (TResponse)executionResult;
    }

    /// <summary>
    /// Handles the core logic of the pipeline decorator.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing 
    /// the response.</returns>
    protected abstract Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default);
}
