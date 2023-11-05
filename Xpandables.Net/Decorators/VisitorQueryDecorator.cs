
/************************************************************************************************************
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
************************************************************************************************************/
using Xpandables.Net.Operations;
using Xpandables.Net.Operations.Messaging;
using Xpandables.Net.Visitors;

namespace Xpandables.Net.Decorators;

/// <summary>
/// This class allows the application author to add visitor support to query control flow.
/// The target query should implement the <see cref="IVisitable{TVisitable}"/> interface in order to activate the behavior.
/// The class decorates the target query handler with an implementation of <see cref="ICompositeVisitor{TElement}"/>
/// and applies all visitors found to the target query before the query get handled. You should provide with implementation
/// of <see cref="IVisitor{TElement}"/>.
/// </summary>
/// <typeparam name="TQuery">Type of query.</typeparam>
/// <typeparam name="TResult">Type of result.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="VisitorQueryDecorator{TQuery, TResult}"/> class with
/// the handler to be decorated and the composite visitor.
/// </remarks>
/// <param name="decoratee">The query to be decorated.</param>
/// <param name="visitor">The composite visitor to apply</param>
/// <exception cref="ArgumentNullException">The <paramref name="decoratee"/> is null.</exception>
/// <exception cref="ArgumentNullException">The <paramref name="visitor"/> is null.</exception>
public sealed class VisitorQueryDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> decoratee,
    ICompositeVisitor<TQuery> visitor) : IQueryHandler<TQuery, TResult>
    where TQuery : notnull, IQuery<TResult>, IVisitable
{
    private readonly IQueryHandler<TQuery, TResult> _decoratee = decoratee
        ?? throw new ArgumentNullException(nameof(decoratee));
    private readonly ICompositeVisitor<TQuery> _visitor = visitor
        ?? throw new ArgumentNullException(nameof(visitor));

    /// <summary>
    /// Asynchronously applies visitor before handling the specified query and returns the task result.
    /// </summary>
    /// <param name="query">The query to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="query"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>A task that represents an object of <see cref="IOperationResult{TValue}"/>.</returns>
    public async ValueTask<OperationResult<TResult>> HandleAsync(
        TQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await query.AcceptAsync(_visitor).ConfigureAwait(false);

        return await _decoratee.HandleAsync(query, cancellationToken).ConfigureAwait(false);
    }
}