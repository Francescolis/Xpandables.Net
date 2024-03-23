﻿
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

using Xpandables.Net.Operations;

namespace Xpandables.Net.Commands;

internal sealed class QueryHandlerWrapper<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> decoratee)
    : IQueryHandlerWrapper<TResult>
    where TQuery : notnull, IQuery<TResult>
{
    public async ValueTask<IOperationResult<TResult>> HandleAsync(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
        => await decoratee.HandleAsync(
            (TQuery)query,
            cancellationToken)
        .ConfigureAwait(false);
}