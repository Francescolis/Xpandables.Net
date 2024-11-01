
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
using Xpandables.Net.Operations;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Responsibilities.Decorators;

/// <summary>
/// Marker interface to indicate that a request should use a unit of work
/// whatever the outcome of the request.
/// </summary>
public interface IUseUnitOfWork { }

/// <summary>
/// A decorator that ensures the unit of work pattern is applied to the 
/// pipeline whatever the outcome of the request.
/// </summary>
/// <param name="unitOfWork">The unit of work instance.</param>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class UnitOfWorkPipelineDecorator<TRequest, TResponse>(
    IUnitOfWork unitOfWork) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IUseUnitOfWork
    where TResponse : IOperationResult
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            TResponse response = await next().ConfigureAwait(false);

            try
            {
                _ = await unitOfWork
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (InvalidOperationException exception)
            {
                return MatchResponse(exception.ToOperationResult());
            }

            return response;
        }
        catch (Exception exception)
        {
            try
            {
                _ = await unitOfWork
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                return MatchResponse(exception.ToOperationResult());

            }
            catch (Exception ex)
            {
                AggregateException aggregateException = new(exception, ex);
                return MatchResponse(aggregateException.ToOperationResult());
            }
        }
    }
}
