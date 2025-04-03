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
using Xpandables.Net.Executions;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Represents a composite validator that validates an instance of 
/// <typeparamref name="TArgument"/> using multiple validators.
/// </summary>
/// <typeparam name="TArgument">The type of the argument to validate.</typeparam>
/// <param name="validators">The validators to use for the validation.</param>
public sealed class CompositeValidator<TArgument>(
    IEnumerable<IValidator<TArgument>> validators) :
    Validator<TArgument>, ICompositeValidator<TArgument>
    where TArgument : class, IValidationEnabled
{
    private readonly IEnumerable<IValidator<TArgument>> _validators = validators
        ?? throw new ArgumentNullException(nameof(validators));

    /// <inheritdoc/>
    public override ExecutionResult Validate(TArgument instance)
    {
        IExecutionResultFailureBuilder failureBuilder =
            ExecutionResults.Failure(System.Net.HttpStatusCode.BadRequest);

        foreach (IValidator<TArgument> validator in _validators
            .OrderBy(o => o.Order))
        {
            ExecutionResult result = validator.Validate(instance);
            if (result.IsFailureStatusCode())
            {
                failureBuilder = failureBuilder.Merge(result);
            }
        }

        ExecutionResult failureResult = failureBuilder.Build();
        return failureResult.Errors.Any()
            ? failureResult
            : ExecutionResults.Ok().Build();
    }

    /// <inheritdoc/>
    public override async ValueTask<ExecutionResult> ValidateAsync(TArgument instance)
    {
        IExecutionResultFailureBuilder failureBuilder =
            ExecutionResults.Failure(System.Net.HttpStatusCode.BadRequest);

        foreach (IValidator<TArgument> validator in _validators
            .OrderBy(o => o.Order))
        {
            ExecutionResult result = await validator
                .ValidateAsync(instance)
                .ConfigureAwait(false);

            if (result.IsFailureStatusCode())
            {
                failureBuilder = failureBuilder.Merge(result);
            }
        }

        ExecutionResult failureResult = failureBuilder.Build();
        return failureResult.Errors.Any()
            ? failureResult
            : ExecutionResults.Ok().Build();
    }
}
