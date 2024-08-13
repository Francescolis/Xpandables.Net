
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

using Xpandables.Net.Distribution;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Visitors;

namespace Xpandables.Net.Decorators;

/// <summary>
/// This class allows the application author to add visitor 
/// support to request control flow.
/// The target request should implement the 
/// <see cref="IVisitable{TVisitable}"/> interface in order to activate the 
/// behavior. The class decorates the target request handler 
/// with an implementation of <see cref="ICompositeVisitor{TElement}"/>
/// and applies all visitors found to the target request 
/// before the request get handled. You should provide with implementation
/// of <see cref="IVisitor{TElement}"/>.
/// </summary>
/// <typeparam name="TRequest">Type of request.</typeparam>
/// <typeparam name="TResponse">Type of result.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="RequestVisitorHandlerDecorator{TRequest, TResponse}"/> class with
/// the handler to be decorated and the composite visitor.
/// </remarks>
/// <param name="decoratee">The request to be decorated.</param>
/// <param name="visitor">The composite visitor to apply</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="visitor"/> is null.</exception>
public sealed class RequestVisitorHandlerDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> decoratee,
    ICompositeVisitor<TRequest> visitor) :
    IRequestHandler<TRequest, TResponse>, IDecorator
    where TRequest : notnull, IRequest<TResponse>, IVisitable
{
    /// <summary>
    /// Asynchronously applies visitor before handling 
    /// the specified request and returns the task result.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <returns>A task that represents an object 
    /// of <see cref="IOperationResult{TValue}"/>.</returns>
    public async Task<IOperationResult<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await request
            .AcceptAsync(visitor)
            .ConfigureAwait(false);

        return await decoratee
            .HandleAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }
}

/// <summary>
/// This class allows the application author to add visitor support 
/// to request control flow.
/// The target request should implement the <see cref="IVisitable{TVisitable}"/> 
/// interface in order to activate the behavior.
/// The class decorates the target request handler with an implementation 
/// of <see cref="ICompositeVisitor{TElement}"/>
/// and applies all visitors found to the target request before the request 
/// get handled. You should provide with implementation
/// of <see cref="IVisitor{TElement}"/>.
/// </summary>
/// <typeparam name="TRequest">Type of the request.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="RequestVisitorHandlerDecorator{TRequest}"/> class with
/// the handler to be decorated and the composite visitor.
/// </remarks>
/// <param name="decoratee">the decorated request handler.</param>
/// <param name="visitor">the visitor to be applied.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="visitor"/> is null.</exception>
public sealed class RequestVisitorHandlerDecorator<TRequest>(
    IRequestHandler<TRequest> decoratee,
    ICompositeVisitor<TRequest> visitor) :
    IRequestHandler<TRequest>, IDecorator
    where TRequest : notnull, IRequest, IVisitable
{
    /// <summary>
    /// Asynchronously applies visitor and handles the specified request.
    /// </summary>
    /// <param name="request">The request instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an object 
    /// of <see cref="IOperationResult"/>.</returns>
    public async Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await request
            .AcceptAsync(visitor)
            .ConfigureAwait(false);

        return await decoratee
            .HandleAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }
}

/// <summary>
/// This class allows the application author to add visitor support to 
/// request control flow.
/// The target request should implement the <see cref="IVisitable{TVisitable}"/> 
/// interface in order to activate the behavior.
/// The class decorates the target request handler with an implementation 
/// of <see cref="ICompositeVisitor{TElement}"/>
/// and applies all visitors found to the target request before the request 
/// get handled. You should provide with implementation
/// of <see cref="IVisitor{TElement}"/>.
/// </summary>
/// <typeparam name="TRequest">Type of request.</typeparam>
/// <typeparam name="TResponse">Type of result.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="AsyncRequestVisitorHandlerDecorator{TRequest, TResponse}"/> class with
/// the request handler to be decorated and the composite visitor.
/// </remarks>
/// <param name="decoratee">The request to be decorated.</param>
/// <param name="visitor">The composite visitor to apply</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="visitor"/> is null.</exception>
public sealed class AsyncRequestVisitorHandlerDecorator<TRequest, TResponse>(
    IAsyncRequestHandler<TRequest, TResponse> decoratee,
    ICompositeVisitor<TRequest> visitor) :
    IAsyncRequestHandler<TRequest, TResponse>, IDecorator
    where TRequest : notnull, IAsyncRequest<TResponse>, IVisitable
{
    /// <summary>
    /// Asynchronously applies visitor before handling the request and 
    /// returns an asynchronous result type.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <returns>An enumerator of <typeparamref name="TResponse"/> 
    /// that can be asynchronously enumerable.</returns>
    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        CheckParameters();

        await request
            .AcceptAsync(visitor)
            .ConfigureAwait(false);

        await foreach (TResponse result
            in decoratee.HandleAsync(request, cancellationToken))
        {
            yield return result;
        }

        void CheckParameters() => ArgumentNullException.ThrowIfNull(request);
    }
}