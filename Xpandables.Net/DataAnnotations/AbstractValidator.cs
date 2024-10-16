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
using Xpandables.Net.Operations;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Provides an abstract base class for implementing validators.
/// </summary>
/// <typeparam name="TArgument">The type of the argument to validate.</typeparam>
public abstract class AbstractValidator<TArgument> : IValidator<TArgument>
    where TArgument : class, IUseValidation
{
    /// <inheritdoc/>
    public virtual int Order => 0;

    /// <summary>
    /// Validates the specified instance and returns the result of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>An <see cref="IOperationResult"/> representing the result of 
    /// the validation.</returns>
    public abstract IOperationResult Validate(TArgument instance);

    /// <summary>
    /// Asynchronously validates the specified instance and returns the result 
    /// of the validation.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the result of 
    /// the validation.</returns>
    public virtual ValueTask<IOperationResult> ValidateAsync(TArgument instance)
    {
        IOperationResult result = Validate(instance);
        return new ValueTask<IOperationResult>(result);
    }
}
