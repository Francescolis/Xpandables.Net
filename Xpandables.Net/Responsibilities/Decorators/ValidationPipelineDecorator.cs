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
using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Responsibilities.Decorators;

/// <summary>
/// A decorator that validates the request before passing it to the next 
/// handler in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class ValidationPipelineDecorator<TRequest, TResponse>(
    ICompositeValidator<TRequest> validators) :
    PipelineDecorator<TRequest, TResponse>
    where TRequest : class, IApplyValidation
    where TResponse : IOperationResult
{
    /// <inheritdoc/>
    protected override async Task<TResponse> HandleCoreAsync(
        TRequest query,
        RequestHandler<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        IOperationResult result = await validators.ValidateAsync(query)
            .ConfigureAwait(false);

        if (!result.IsSuccessStatusCode)
        {
            return MatchResponse(result);
        }

        return await next().ConfigureAwait(false);
    }
}