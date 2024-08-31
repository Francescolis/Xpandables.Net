
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Distribution.Internals;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Distribution;

/// <summary>
/// Provides with extension method for the distributor.
/// </summary>
public static class DistributorExtensions
{
    /// <summary>
    /// Asynchronously send the aggregate from the request using
    /// the <see cref="IRequestAggregateHandler{TRequest, TAggregate}"/> implementation.
    /// </summary>
    /// <typeparam name="TAggregate">Type of the response.</typeparam>
    /// <param name="distributor">The target distributor instance.</param>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while 
    /// waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> 
    /// is null.</exception>
    /// <returns>A task that represents an 
    /// <see cref="IOperationResult{TValue}"/>.</returns>
    public static async Task<IOperationResult> SendAsync<TAggregate>(
        this IDistributor distributor,
        IRequestAggregate<TAggregate> request,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IAggregate
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(distributor);

        try
        {
            Type requestWrapperType = typeof(RequestAggregateHandlerWrapper<,>)
                .MakeGenericType(request.GetType(), typeof(TAggregate));

            IRequestAggregateHandlerWrapper<TAggregate> handler =
                (IRequestAggregateHandlerWrapper<TAggregate>)distributor
                .GetRequiredService(requestWrapperType);

            return await handler
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                            and not OperationResultException)
        {
            return OperationResults
                .InternalError<TAggregate>()
                .WithException(exception)
                .Build();
        }
    }

    /// <summary>
    /// Asynchronously fetches the response from the request using
    /// the <see cref="IRequestHandler{TRequest, TResponse}"/> implementation and 
    /// returns a response.
    /// </summary>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <param name="distributor">The target distributor instance.</param>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while 
    /// waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> 
    /// is null.</exception>
    /// <returns>A task that represents an 
    /// <see cref="IOperationResult{TValue}"/>.</returns>
    public static async Task<IOperationResult<TResponse>> GetAsync<TResponse>(
        this IDistributor distributor,
        IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(distributor);

        try
        {
            Type requestWrapperType = typeof(RequestResponseHandlerWrapper<,>)
                .MakeGenericType(request.GetType(), typeof(TResponse));

            IRequestHandlerWrapper<TResponse> handler =
                (IRequestHandlerWrapper<TResponse>)distributor
                .GetRequiredService(requestWrapperType);

            return await handler.HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                            and not OperationResultException)
        {
            return OperationResults
                .InternalError<TResponse>()
                .WithException(exception)
                .Build();
        }
    }

    /// <summary>
    /// Asynchronously fetches the response from the request
    /// using the <see cref="IAsyncRequestHandler{TRequest, TResponse}"/> 
    /// implementation and returns an enumerator of 
    /// <typeparamref name="TResponse"/> that can be asynchronously enumerated.
    /// </summary>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <param name="distributor">The target distributor instance.</param>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while
    /// waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> 
    /// is null.</exception>
    /// <exception cref="OperationResultException">The operation 
    /// failed.</exception>
    /// <returns>An enumerator of <typeparamref name="TResponse"/> that can be 
    /// asynchronously enumerated.</returns>
    public static IAsyncEnumerable<TResponse> FetchAsync<TResponse>(
        this IDistributor distributor,
        IAsyncRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(distributor);

        try
        {
            Type requestWrapperType = typeof(AsyncRequestResponseHandlerWrapper<,>)
                .MakeGenericType(request.GetType(), typeof(TResponse));

            IAsyncRequestHandlerWrapper<TResponse> handler =
                (IAsyncRequestHandlerWrapper<TResponse>)distributor
                .GetRequiredService(requestWrapperType);

            return handler.HandleAsync(request, cancellationToken);
        }
        catch (Exception exception) when (exception is not ArgumentNullException
                                            and not OperationResultException)
        {
            throw new OperationResultException(exception.ToOperationResult());
        }
    }
}
