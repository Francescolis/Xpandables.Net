
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

using Xpandables.Net.Operations;
using Xpandables.Net.Validators;

namespace Xpandables.Net.Decorators;

/// <summary>
/// This class allows the application author to add validation 
/// support to request control flow.
/// The target request should implement the <see cref="IValidateDecorator"/> 
/// interface 
/// in order to activate the behavior.
/// The class decorates the target request handler with an implementation 
/// of <see cref="ICompositeValidator{TArgument}"/>
/// and applies all validators found to the target request before the 
/// request get handled if there is no error.
/// You should provide with implementation
/// of <see cref="IValidator{TArgument}"/> for validation.
/// </summary>
/// <typeparam name="TRequest">Type of the request.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="RequestValidatorHandlerDecorator{TRequest}"/> class
/// with the handler to be decorated and the composite validator.
/// </remarks>
/// <param name="decoratee">The request handler to be decorated.</param>
/// <param name="validator">The validator instance.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="validator"/> is null.</exception>
public sealed class RequestValidatorHandlerDecorator<TRequest>(
    IRequestHandler<TRequest> decoratee,
    ICompositeValidator<TRequest> validator) :
    IRequestHandler<TRequest>, IDecorator
    where TRequest : notnull, IRequest, IValidateDecorator
{
    /// <summary>
    /// Asynchronously validates the request before handling 
    /// if there is no error.
    /// </summary>
    /// <param name="request">The request instance to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request" /> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <returns>A task that represents an <see cref="OperationResult"/>
    /// .</returns>
    public async Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        IOperationResult operation = await validator
            .ValidateAsync(request)
            .ConfigureAwait(false);

        return operation.IsFailure
            ? operation
            : await decoratee
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);
    }
}

/// <summary>
/// This class allows the application author to add validation support 
/// to request control flow.
/// The target request should implement the <see cref="IValidateDecorator"/> 
/// interface in order to activate the behavior.
/// The class decorates the target request handler with an implementation 
/// of <see cref="ICompositeValidator{TArgument}"/>
/// and applies all validators found to the target request before 
/// the request get handled.
/// You should provide with implementation
/// of <see cref="IValidator{TArgument}"/>for validation.
/// </summary>
/// <typeparam name="TRequest">Type of request.</typeparam>
/// <typeparam name="TResponse">Type of result.</typeparam>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="RequestValidatorHandlerDecorator{TRequest, TResponse}"/> class with
/// the handler to be decorated and the composite validator.
/// </remarks>
/// <param name="decoratee">The request handler to decorate.</param>
/// <param name="validator">The validator instance.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="validator"/> is null.</exception>
public sealed class RequestValidatorHandlerDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> decoratee,
    ICompositeValidator<TRequest> validator) :
    IRequestHandler<TRequest, TResponse>, IDecorator
    where TRequest : notnull, IRequest<TResponse>, IValidateDecorator
{
    /// <summary>
    /// Asynchronously validates the request before handling 
    /// and returns the task result.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <returns>A task that represents an object 
    /// of <see cref="IOperationResult{TValue}"/>.</returns>
    public async Task<IOperationResult<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        IOperationResult operation = await validator
            .ValidateAsync(request)
            .ConfigureAwait(false);

        return operation.IsFailure
            ? operation.ToOperationResult<TResponse>()
            : await decoratee
                .HandleAsync(request, cancellationToken)
                .ConfigureAwait(false);
    }
}

/// <summary>
/// This class allows the application author to add validation
/// support to request control flow.
/// The target request should implement the <see cref="IValidateDecorator"/> 
/// interface in order to activate the behavior.
/// The class decorates the target request handler with an 
/// implementation of <see cref="ICompositeValidator{TArgument}"/>
/// and applies all validators found for the target request before 
/// the request get handled.
/// If a validator is failed, returns an empty enumerable.
/// You should provide with implementation of 
/// <see cref="IValidator{TArgument}"/> for validation.
/// </summary>
/// <typeparam name="TRequest">Type of request.</typeparam>
/// <typeparam name="TResponse">Type of result.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="AsyncRequestValidatorHandlerDecorator{TRequest, TResponse}"/> class with
/// the handler to be decorated and the composite validator.
/// </remarks>
/// <param name="decoratee">The request handler to decorate.</param>
/// <param name="validator">The validator instance.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="validator"/> is null.</exception>
public sealed class AsyncRequestValidatorHandlerDecorator<TRequest, TResponse>(
    IAsyncRequestHandler<TRequest, TResponse> decoratee,
    ICompositeValidator<TRequest> validator) :
    IAsyncRequestHandler<TRequest, TResponse>, IDecorator
    where TRequest : notnull, IAsyncRequest<TResponse>, IValidateDecorator
{
    /// <summary>
    /// Asynchronously validates the request before handling 
    /// and returns an asynchronous result type.
    /// if a validator is failed returns an empty enumerable.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <exception cref="OperationResultException">The validation failed
    /// .</exception>
    /// <returns>An enumerator of <typeparamref name="TResponse"/> 
    /// that can be asynchronously enumerable.</returns>
    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IOperationResult operation = await validator
            .ValidateAsync(request)
            .ConfigureAwait(false);

        if (operation.IsFailure)
        {
            throw new OperationResultException(operation);
        }

        await foreach (TResponse result in decoratee
            .HandleAsync(request, cancellationToken)
            .ConfigureAwait(false))
        {
            yield return result;
        }
    }
}