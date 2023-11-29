/************************************************************************************************************
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
************************************************************************************************************/
using Xpandables.Net.Operations;

namespace Xpandables.Net.Validators;

/// <summary>
/// The composite validation class used to wrap all validators for a specific type.
/// </summary>
/// <typeparam name="TArgument">Type of the argument to be validated</typeparam>
/// <remarks>
/// Initializes the composite validation with all validation instances for the argument.
/// </remarks>
/// <param name="validationInstances">The collection of validators to act with.</param>
public sealed class CompositeValidator<TArgument>(IEnumerable<IValidator<TArgument>> validationInstances)
    : ICompositeValidator<TArgument>
    where TArgument : notnull
{
    private readonly IEnumerable<IValidator<TArgument>> _validationInstances = validationInstances;

    /// <summary>
    /// Validates the argument and returns validation state with errors if necessary.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="argument"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>Returns a result state that contains validation information.</returns>
    public OperationResult Validate(TArgument argument)
    {
        foreach (var validator in _validationInstances.OrderBy(o => o.Order))
        {
            OperationResult operation = validator.Validate(argument);
            if (operation.IsFailure)
                return operation;
        }

        return OperationResults.Ok().Build();
    }

    /// <summary>
    /// Asynchronously validates the argument and returns validation state with errors if necessary.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="argument"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The operation failed. See inner exception.</exception>
    /// <returns>Returns a result state that contains validation information.</returns>
    public async ValueTask<OperationResult> ValidateAsync(TArgument argument)
    {
        foreach (var validator in _validationInstances.OrderBy(o => o.Order))
        {
            OperationResult operation = await validator
                .ValidateAsync(argument)
                .ConfigureAwait(false);

            if (operation.IsFailure)
                return operation;
        }

        return OperationResults.Ok().Build();
    }
}
