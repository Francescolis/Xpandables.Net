
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

namespace Xpandables.Net.Distribution.Internals;

internal sealed class RequestResponseHandlerWrapper<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> decoratee)
    : IRequestHandlerWrapper<TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    public async Task<IOperationResult<TResponse>> HandleAsync(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
        => await decoratee.HandleAsync(
            (TRequest)request,
            cancellationToken)
        .ConfigureAwait(false);
}