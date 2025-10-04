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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net.ExecutionResults;
using System.Net.Validators;

using Xpandables.Net.Tasks;
using Xpandables.Net.Validators;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Net.Tasks.Pipelines;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler nextHandler,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nextHandler);

        IReadOnlyCollection<ValidationResult> validationResults = await validators
            .ValidateAsync(context.Request)
            .ConfigureAwait(false);

        if (validationResults.Count != 0)
        {
            return validationResults.ToExecutionResult();
        }

        return await nextHandler(cancellationToken).ConfigureAwait(false);
    }
}