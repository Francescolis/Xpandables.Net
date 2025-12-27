/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides a validator that combines multiple validators and applies them to a single argument instance.
/// </summary>
/// <remarks>The composite validator executes each contained validator in sequence and aggregates all validation
/// results. This allows for modular validation logic by composing multiple validators. The order of validators in the
/// collection determines the order in which they are applied.</remarks>
/// <typeparam name="TArgument">The type of the object to validate. Must be a reference type that implements <see cref="IRequiresValidation"/>.</typeparam>
/// <param name="validators">The collection of validators to apply to the argument instance. Cannot be null.</param>
public sealed class CompositeValidator<TArgument>(IEnumerable<IValidator<TArgument>> validators) :
    Validator<TArgument>, ICompositeValidator<TArgument>
    where TArgument : class, IRequiresValidation
{
    private readonly IEnumerable<IValidator<TArgument>> _validators = validators
        ?? throw new ArgumentNullException(nameof(validators));

    /// <inheritdoc/>
    public override IReadOnlyCollection<ValidationResult> Validate(TArgument instance)
    {
        List<ValidationResult> validationResults = [];
        foreach (IValidator<TArgument> validator in _validators)
        {
            var results = validator.Validate(instance);
            if (results is { Count: > 0 })
            {
                validationResults.AddRange(results);
            }
        }

        return validationResults;
    }

    /// <inheritdoc/>
    public override async ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(TArgument instance)
    {
        List<ValidationResult> validationResults = [];

        foreach (IValidator<TArgument> validator in _validators)
        {
            var results = await validator
                .ValidateAsync(instance)
                .ConfigureAwait(false);

            if (results is { Count: > 0 })
            {
                validationResults.AddRange(results);
            }
        }

        return validationResults;
    }
}
