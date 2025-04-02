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
/// Provides methods to validate an instance and return the result 
/// of the validation.
/// </summary>
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
    /// <returns>An <see cref="IExecutionResult"/> representing the result of 
    /// the validation.</returns>
    /// <exception cref="ExecutionResultException">thrown when unable to return
    /// IExecutionResult.</exception>
    IExecutionResult Validate(object instance);

    /// <summary>
    /// Asynchronously validates the specified instance and returns the result 
    /// of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the result of 
    /// the validation.</returns>
    /// <exception cref="ExecutionResultException">thrown when unable to return
    /// IExecutionResult.</exception>
    public ValueTask<IExecutionResult> ValidateAsync(object instance)
    {
        IExecutionResult result = Validate(instance);
        return new ValueTask<IExecutionResult>(result);
    }
}

/// <summary>
/// Provides methods to validate an instance of type <typeparamref name="TArgument"/> 
/// and return the result of the validation.
/// </summary>
/// <typeparam name="TArgument">The type of the instance to validate.</typeparam>
public interface IValidator<in TArgument> : IValidator
    where TArgument : class, IValidationEnabled
{
    /// <summary>
    /// Validates the specified instance and returns the result of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>An <see cref="IExecutionResult"/> representing the result of 
    /// the validation.</returns>
    /// <exception cref="ExecutionResultException">thrown when unable to return
    /// IExecutionResult.</exception>
    IExecutionResult Validate(TArgument instance);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IExecutionResult IValidator.Validate(object instance) =>
        Validate((TArgument)instance);

    /// <summary>
    /// Asynchronously validates the specified instance and returns the result 
    /// of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the result of 
    /// the validation.</returns>
    /// <exception cref="ExecutionResultException">thrown when unable to return
    /// IExecutionResult.</exception>
    ValueTask<IExecutionResult> ValidateAsync(TArgument instance);

    [EditorBrowsable(EditorBrowsableState.Never)]
    ValueTask<IExecutionResult> IValidator.ValidateAsync(object instance) =>
        ValidateAsync((TArgument)instance);
}