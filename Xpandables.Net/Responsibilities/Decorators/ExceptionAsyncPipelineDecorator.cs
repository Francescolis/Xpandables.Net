﻿/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

namespace Xpandables.Net.Responsibilities.Decorators;

/// <summary>
/// A decorator for handling exceptions in an asynchronous pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class ExceptionAsyncPipelineDecorator<TRequest, TResponse> :
    AsyncPipelineDecorator<TRequest, TResponse>
    where TRequest : class, IQueryAsync<TResponse>
{
    /// <inheritdoc/>
    protected override async IAsyncEnumerable<TResponse> HandleCoreAsync(
        TRequest request,
        RequestAsyncHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<TResponse> enumerator =
            next().GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            try
            {
                if (!await enumerator
                    .MoveNextAsync(cancellationToken)
                    .ConfigureAwait(false))
                {
                    break;
                }
            }
            catch (Exception exception)
            {
                throw new OperationResultException(
                    exception.ToOperationResult());
            }

            yield return enumerator.Current;
        }
    }
}
