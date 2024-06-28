
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

using Xpandables.Net.Commands;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Validators;

namespace Xpandables.Net.Decorators;

/// <summary>
/// This class allows the application author to add validation
/// support to query control flow.
/// The target query should implement the <see cref="IValidateDecorator"/> 
/// interface in order to activate the behavior.
/// The class decorates the target query handler with an 
/// implementation of <see cref="ICompositeValidator{TArgument}"/>
/// and applies all validators found for the target query before 
/// the command get handled.
/// If a validator is failed, returns an empty enumerable.
/// You should provide with implementation of 
/// <see cref="IValidator{TArgument}"/> for validation.
/// </summary>
/// <typeparam name="TQuery">Type of query.</typeparam>
/// <typeparam name="TResult">Type of result.</typeparam>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="ValidatorAsyncQueryDecorator{TQuery, TResult}"/> class with
/// the handler to be decorated and the composite validator.
/// </remarks>
/// <param name="decoratee">The query handler to decorate.</param>
/// <param name="validator">The validator instance.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="validator"/> is null.</exception>
public sealed class ValidatorAsyncQueryDecorator<TQuery, TResult>(
    IAsyncQueryHandler<TQuery, TResult> decoratee,
    ICompositeValidator<TQuery> validator) :
    IAsyncQueryHandler<TQuery, TResult>, IDecorator
    where TQuery : notnull, IAsyncQuery<TResult>, IValidateDecorator
{
    /// <summary>
    /// Asynchronously validates the query before handling 
    /// and returns an asynchronous result type.
    /// if a validator is failed returns an empty enumerable.
    /// </summary>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="query"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. 
    /// See inner exception.</exception>
    /// <exception cref="OperationResultException">The validation failed
    /// .</exception>
    /// <returns>An enumerator of <typeparamref name="TResult"/> 
    /// that can be asynchronously enumerable.</returns>
    public async IAsyncEnumerable<TResult> HandleAsync(
        TQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IOperationResult operation = await validator
            .ValidateAsync(query)
            .ConfigureAwait(false);

        if (operation.IsFailure)
        {
            throw new OperationResultException(operation);
        }

        await foreach (TResult result in decoratee
            .HandleAsync(query, cancellationToken)
            .ConfigureAwait(false))
        {
            yield return result;
        }
    }
}
