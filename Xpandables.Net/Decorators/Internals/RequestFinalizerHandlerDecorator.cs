
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
using System.Runtime.CompilerServices;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Decorators.Internals;
internal sealed class RequestFinalizerHandlerDecorator<TRequest>(
    IRequestHandler<TRequest> decoratee,
    IOperationFinalizer operationResultFinalizer)
    : IRequestHandler<TRequest>, IDecorator
    where TRequest : notnull, IRequest, IOperationFinalizerDecorator
{
    public async Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IOperationResult response = await decoratee
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return operationResultFinalizer.Finalizer is not null
                ? operationResultFinalizer.Finalizer.Invoke(response)
                : response;
        }
        catch (OperationResultException resultException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer
                    .Invoke(resultException.Operation)
                : resultException.Operation;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer
                    .Invoke(exception.ToOperationResult())
                : OperationResults
                .InternalError()
                .WithTitle("RequestFinalizerHandlerDecorator")
                .WithException(exception)
                .Build();
        }
    }
}

internal sealed class RequestAggregateFinalizerHandlerDecorator<TRequest, TAggregate>(
    IRequestAggregateHandler<TRequest, TAggregate> decoratee,
    IOperationFinalizer operationResultFinalizer)
    : IRequestAggregateHandler<TRequest, TAggregate>, IDecorator
    where TRequest : class, IRequestAggregate<TAggregate>, IOperationFinalizerDecorator
    where TAggregate : class, IAggregate
{
    public async Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IOperationResult response = await decoratee
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return operationResultFinalizer.Finalizer is not null
                ? operationResultFinalizer.Finalizer.Invoke(response)
                : response;
        }
        catch (OperationResultException resultException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer
                    .Invoke(resultException.Operation)
                : resultException.Operation;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer
                    .Invoke(exception.ToOperationResult())
                : OperationResults
                .InternalError()
                .WithTitle("RequestAggregateFinalizerHandlerDecorator")
                .WithException(exception)
                .Build();
        }
    }
}

internal sealed class RequestFinalizerHandlerDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> decoratee,
    IOperationFinalizer operationResultFinalizer)
    : IRequestHandler<TRequest, TResponse>, IDecorator
    where TRequest : notnull, IRequest<TResponse>, IOperationFinalizerDecorator
{
    public async Task<IOperationResult<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IOperationResult<TResponse> response = await decoratee
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return operationResultFinalizer.Finalizer is not null
                ? operationResultFinalizer.Finalizer
                    .Invoke(response)
                    .ToOperationResult<TResponse>()
                : response;
        }
        catch (OperationResultException resultException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer.Invoke(resultException.Operation)
                    .ToOperationResult<TResponse>()
                : resultException.Operation
                .ToOperationResult<TResponse>();
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return operationResultFinalizer.CallFinalizerOnException
                ? operationResultFinalizer
                    .Finalizer.Invoke(exception.ToOperationResult())
                    .ToOperationResult<TResponse>()
                : OperationResults
                .InternalError<TResponse>()
                .WithTitle("RequestFinalizerHandlerDecorator")
                .WithException(exception)
                .Build();
        }
    }
}

internal sealed class AsyncRequestFinalizerHandlerDecorator<TRequest, TResponse>(
    IAsyncRequestHandler<TRequest, TResponse> decoratee,
    IOperationFinalizer operationResultFinalizer)
    : IAsyncRequestHandler<TRequest, TResponse>, IDecorator
    where TRequest : notnull, IAsyncRequest<TResponse>,
    IOperationFinalizerDecorator
{
    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // the awaited task
        await using IAsyncEnumerator<TResponse> asyncEnumerator = decoratee
            .HandleAsync(request, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        // the awaited task


        TResponse? response = default;
        bool finalResponse = false;

        for (bool responseExist = true; responseExist;)
        {
            try
            {
                responseExist = await asyncEnumerator
                    .MoveNextAsync()
                    .ConfigureAwait(false);

                if (responseExist)
                {
                    response = asyncEnumerator.Current;
                }
            }
            catch (Exception exception)
            {
                if (operationResultFinalizer.CallFinalizerOnException)
                {
                    IOperationResult<TResponse> finalizerResult
                        = operationResultFinalizer
                        .Finalizer
                        .Invoke(exception.ToOperationResult())
                        .ToOperationResult<TResponse>();

                    if (finalizerResult.IsFailure)
                    {
                        throw new OperationResultException(finalizerResult);
                    }

                    if (finalizerResult.Result is not null)
                    {
                        response = finalizerResult.Result;
                        responseExist = true;
                        finalResponse = true;
                    }
                    else
                    {
                        responseExist = false;
                    }
                }
                else
                {
                    throw;
                }
            }

            if (responseExist)
            {
                yield return response!;
                if (finalResponse)
                {
                    yield break;
                }
            }
            else
            {
                yield break;
            }
        }
    }
}