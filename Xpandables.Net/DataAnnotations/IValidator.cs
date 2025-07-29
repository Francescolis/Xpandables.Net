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
using System.ComponentModel;

using Xpandables.Net.Executions;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Defines a contract for validating objects and returning validation results.
/// </summary>
/// <remarks>Implementations of this interface provide mechanisms to validate objects and determine their
/// compliance with specific rules or criteria. The validation can be performed synchronously or asynchronously, and the
/// order of execution can be specified.</remarks>
public interface IValidator
{
    /// <summary>
    /// Gets the order of the validator.
    /// </summary>
    public virtual int Order => 0;

    /// <summary>
    /// Validates the specified instance and returns the result of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>An <see cref="ExecutionResult"/> representing the result of 
    /// the validation.</returns>
    /// <exception cref="ExecutionResultException">thrown when unable to return
    /// ExecutionResult.</exception>
    ExecutionResult Validate(object instance);

    /// <summary>
    /// Validates the specified instance and returns the result 
    /// of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the result of 
    /// the validation.</returns>
    /// <exception cref="ExecutionResultException">thrown when unable to return
    /// ExecutionResult.</exception>
    public ValueTask<ExecutionResult> ValidateAsync(object instance)
    {
        ExecutionResult result = Validate(instance);
        return new ValueTask<ExecutionResult>(result);
    }
}

/// <summary>
/// Defines a contract for validating instances of a specified type.
/// </summary>
/// <remarks>This interface provides synchronous and asynchronous methods for validating instances of the
/// specified type. Implementations should ensure that the validation logic is encapsulated within these methods,
/// returning an <see cref="ExecutionResult"/> that indicates the outcome of the validation process.</remarks>
/// <typeparam name="TArgument">The type of the instance to be validated. Must be a class that implements <see cref="IRequiresValidation"/>.</typeparam>
public interface IValidator<in TArgument> : IValidator
    where TArgument : class, IRequiresValidation
{
    /// <summary>
    /// Validates the specified instance and returns the result of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>An <see cref="ExecutionResult"/> representing the result of 
    /// the validation.</returns>
    /// <exception cref="ExecutionResultException">thrown when unable to return
    /// ExecutionResult.</exception>
    ExecutionResult Validate(TArgument instance);

    [EditorBrowsable(EditorBrowsableState.Never)]
    ExecutionResult IValidator.Validate(object instance) =>
        Validate((TArgument)instance);

    /// <summary>
    /// Validates the specified instance and returns the result 
    /// of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the result of 
    /// the validation.</returns>
    /// <exception cref="ExecutionResultException">thrown when unable to return
    /// ExecutionResult.</exception>
    ValueTask<ExecutionResult> ValidateAsync(TArgument instance);

    [EditorBrowsable(EditorBrowsableState.Never)]
    ValueTask<ExecutionResult> IValidator.ValidateAsync(object instance) =>
        ValidateAsync((TArgument)instance);
}