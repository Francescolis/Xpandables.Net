
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
using Xpandables.Net.Distribution;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Decorators;

/// <summary>
/// Represents a method signature to be used to apply 
/// persistence behavior to a request task.
/// </summary>
/// <param name="cancellationToken">A CancellationToken 
/// to observe while waiting for the task to complete.</param>
/// <returns>A task that represents an <see cref="IOperationResult"/>.</returns>
/// <exception cref="InvalidOperationException">The persistence operation
/// failed to execute.</exception>
public delegate Task<IOperationResult> PersistenceRequestDelegate(
    CancellationToken cancellationToken);

/// <summary>
/// This class allows the application author to add persistence 
/// support to request control flow.
/// The target request should implement the <see cref="IPersistenceDecorator"/>
/// interface in order to activate the behavior.
/// The class decorates the target request handler with an definition 
/// of <see cref="PersistenceRequestDelegate"/> 
/// that get called after the main one in the same control flow only.
/// </summary>
/// <typeparam name="TRequest">Type of request.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="RequestPersistenceHandlerDecorator{TRequest}"/> class with
/// the decorated handler and the unit of work to act on.
/// </remarks>
/// <param name="persistenceRequestDelegate">The persistence delegate 
/// to apply persistence.</param>
/// <param name="decoratee">The decorated request handler.</param>
/// <exception cref="ArgumentNullException">The <paramref name="decoratee"/> 
/// or <paramref name="persistenceRequestDelegate"/>
/// is null.</exception>
public sealed class RequestPersistenceHandlerDecorator<TRequest>(
    IRequestHandler<TRequest> decoratee,
    PersistenceRequestDelegate persistenceRequestDelegate)
    : IRequestHandler<TRequest>, IDecorator
    where TRequest : notnull, IRequest, IPersistenceDecorator
{
    /// <summary>
    /// Asynchronously handles the specified request and persists 
    /// changes to store if there is no exception or error.
    /// </summary>
    /// <param name="request">The request instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <returns>A task that represents an 
    /// object of <see cref="IOperationResult"/>.</returns>
    public async Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IOperationResult requestResult = await decoratee
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return requestResult.IsFailure
                ? requestResult
                : await persistenceRequestDelegate(cancellationToken)
                .ConfigureAwait(false)
                    is { IsFailure: true } persistenceResult
                ? persistenceResult
                : requestResult;
        }
        catch (OperationResultException resultException)
        {
            return resultException.Operation;
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return OperationResults
                .InternalError()
                .WithDetail(I18nXpandables.ActionSpecifiedFailedSeeException
                    .StringFormat(nameof(RequestPersistenceHandlerDecorator<TRequest>)))
                .WithException(exception)
                .Build();
        }
    }
}
