
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
using Xpandables.Net.Events;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Internals;

internal sealed class Dispatcher(IServiceProvider serviceProvider)
    : IDispatcher
{
    private readonly IEventPublisher _eventPublisher = serviceProvider
        .GetRequiredService<IEventPublisher>();

    public async Task<IOperationResult<TResponse>> GetAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : notnull, IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            IRequestHandler<TRequest, TResponse> handler =
              serviceProvider
              .GetRequiredService<IRequestHandler<TRequest, TResponse>>();

            return await handler
                .HandleAsync(request, cancellationToken)
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

    public async Task<IOperationResult> SendAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : notnull, IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            IRequestHandler<TRequest> handler =
        serviceProvider
        .GetRequiredService<IRequestHandler<TRequest>>();

            return await handler
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not OperationResultException)
        {
            return OperationResults
                .InternalError()
                .WithException(exception)
                .Build();
        }
    }

    /// <inheritdoc/>>
    public async Task<IOperationResult> SendAsync<TRequest, TAggregate>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IAggregate
        where TRequest : class, IRequestAggregate<TAggregate>
    {
        try
        {
            IRequestAggregateHandler<TRequest, TAggregate> handler =
            serviceProvider
                .GetRequiredService<IRequestAggregateHandler<TRequest, TAggregate>>();

            return await handler
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not OperationResultException)
        {
            return OperationResults
                .InternalError()
                .WithException(exception)
                .Build();
        }
    }

    object? IServiceProvider.GetService(Type serviceType)
        => serviceProvider.GetService(serviceType);

    public IAsyncEnumerable<TResponse> FetchAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : notnull, IAsyncRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            IAsyncRequestHandler<TRequest, TResponse> handler =
        serviceProvider
        .GetRequiredService<IAsyncRequestHandler<TRequest, TResponse>>();

            return handler.HandleAsync(request, cancellationToken);
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException
                and not OperationResultException)
        {
            IOperationResult operationResult = exception.ToOperationResult();
            throw new OperationResultException(operationResult);
        }
    }

    public async Task<IOperationResult> PublishAsync<TEvent>(
        TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent
        => await _eventPublisher
            .PublishAsync(@event, cancellationToken)
            .ConfigureAwait(false);
}