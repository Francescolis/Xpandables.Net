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
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Executions.Pipelines;

/// <summary>
/// Represents a pipeline decorator that performs validation on the incoming request
/// using a composite validator before proceeding to the next pipeline component.
/// </summary>
/// <typeparam name="TRequest">The type of the request object, must be a class and implement <see cref="IRequiresValidation"/>.</typeparam>
/// <param name="validators">The instance of a composite validator responsible for validating the request.</param>
/// <remarks>
/// If the validation fails, the pipeline will short-circuit and return a validation error response.
/// If the validation succeeds, the execution continues to the next component in the pipeline.
/// </remarks>
public sealed class PipelineValidationDecorator<TRequest>(ICompositeValidator<TRequest> validators) :
    IPipelineDecorator<TRequest>
    where TRequest : class, IRequest, IRequiresValidation
{
    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken = default)
    {
        ExecutionResult result = await validators
            .ValidateAsync(context.Request)
            .ConfigureAwait(false);

        if (!result.IsSuccessStatusCode)
        {
            return result;
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}