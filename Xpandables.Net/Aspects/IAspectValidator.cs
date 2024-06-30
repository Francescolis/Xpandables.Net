/*******************************************************************************
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
********************************************************************************/
using System.ComponentModel;

using Xpandables.Net.Operations;

namespace Xpandables.Net.Aspects;

/// <summary>
/// Represents a marker interface that allows the class implementation to be 
/// recognized as an aspect validator that will be called before the method
/// execution.
/// </summary>
public interface IAspectValidator : IAspect
{
    /// <summary>
    /// Validates the argument and returns validation state with errors if 
    /// necessary or throws an <see cref="OperationResultException"/>.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>    
    /// <returns>Returns a result state that contains validation information
    /// .</returns>
    /// <exception cref="OperationResultException">When the validation failed.
    /// </exception>
    IOperationResult Validate(object? argument);
}

/// <summary>
/// Represents a marker interface that allows the class implementation to be 
/// recognized as an aspect validator.
/// </summary>
public interface IAsyncAspectValidator : IAspectValidator
{
    /// <summary>
    /// Validates the argument and returns validation state with errors if 
    /// necessary or throws an <see cref="OperationResultException"/>.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>    
    /// <returns>Returns a result state that contains validation information
    /// .</returns>
    /// <exception cref="OperationResultException">When the validation failed.
    /// </exception>
    Task<IOperationResult> ValidateAsync(object? argument);

#pragma warning disable CA1033 // Interface methods should be callable by child types
    [EditorBrowsable(EditorBrowsableState.Never)]
    IOperationResult IAspectValidator.Validate(object? argument)
#pragma warning restore CA1033 // Interface methods should be callable by child types
    {
        Task<IOperationResult> task = ValidateAsync(argument);
        task.Wait();
        return task.Result;
    }
}


/// <summary>
/// Represents a marker interface that allows the class implementation to be
/// recognized as an aspect validator.
/// </summary>
/// <typeparam name="TArgument">The type of the argument to be validated
/// .</typeparam>
public interface IAspectValidator<TArgument> : IAspectValidator
{
    /// <summary>
    /// Validates the argument and returns validation state with errors if
    /// necessary or throws an <see cref="OperationResultException"/>.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <returns>Returns a result state that contains validation information
    /// </returns>
    /// <exception cref="OperationResultException">When the validation failed.
    /// </exception>
    IOperationResult Validate(TArgument? argument);
    IOperationResult IAspectValidator.Validate(object? argument)
        => Validate((TArgument?)argument);
}

/// <summary>
/// Represents a marker interface that allows the class implementation to be
/// recognized as an aspect validator.
/// </summary>
/// <typeparam name="TArgument">The type of the argument to be validated
/// .</typeparam>
public interface IAsyncAspectValidator<TArgument> : IAsyncAspectValidator
{
    /// <summary>
    /// Validates the argument and returns validation state with errors if
    /// necessary or throws an <see cref="OperationResultException"/>.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <returns>Returns a result state that contains validation information
    /// </returns>
    /// <exception cref="OperationResultException">When the validation failed.
    /// </exception>
    Task<IOperationResult> ValidateAsync(TArgument? argument);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<IOperationResult> IAsyncAspectValidator
        .ValidateAsync(object? argument) => ValidateAsync((TArgument?)argument);
}