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

namespace Xpandables.Net.Decorators;

/// <summary>
/// A decorator that validates the query before passing it to the next 
/// handler in the pipeline.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class ValidationPipelineDecorator<TQuery, TResult>(
    ICompositeValidator<TQuery> validators) :
    IPipelineDecorator<TQuery, TResult>
    where TQuery : class, IUseValidation
    where TResult : class, IOperationResult
{
    /// <inheritdoc/>
    public Task<TResult> HandleAsync(
        TQuery query,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        IOperationResult result = validators.Validate(query);

        if (!result.IsSuccessStatusCode)
        {
            return Task.FromResult((TResult)result);
        }

        return next();
    }
}