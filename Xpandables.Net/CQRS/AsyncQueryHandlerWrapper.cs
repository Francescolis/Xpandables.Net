﻿
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
namespace Xpandables.Net.CQRS;

internal sealed class AsyncQueryHandlerWrapper<TQuery, TResult>(
    IAsyncQueryHandler<TQuery, TResult> decoratee) : IAsyncQueryHandlerWrapper<TResult>
    where TQuery : notnull, IAsyncQuery<TResult>
{
    private readonly IAsyncQueryHandler<TQuery, TResult> _decoratee =
        decoratee ?? throw new ArgumentNullException($"{decoratee} : {nameof(TQuery)}.{nameof(TResult)}");

    public IAsyncEnumerable<TResult> HandleAsync(
        IAsyncQuery<TResult> query, CancellationToken cancellationToken = default)
        => _decoratee.HandleAsync((TQuery)query, cancellationToken);
}