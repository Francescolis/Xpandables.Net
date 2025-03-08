/*******************************************************************************
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
using Xpandables.Net.Executions.Pipelines;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// A wrapper for applying pipeline on requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class PipelineRequestHandler<TRequest, TResponse>(
    IHandler<TRequest, TResponse> decoratee,
    IEnumerable<IPipelineDecorator<TRequest, TResponse>> decorators) :
    IPipelineRequestHandler<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <inheritdoc/>
    public TResponse Handle(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        TResponse result = decorators
            .Reverse()
            .Aggregate<IPipelineDecorator<TRequest, TResponse>,
            RequestHandler<TResponse>>(
                Handler,
                (next, decorator) => () => decorator.Handle(
                    request,
                    next,
                    cancellationToken))();

        return result;

        TResponse Handler() => decoratee.Handle(request, cancellationToken);
    }
}