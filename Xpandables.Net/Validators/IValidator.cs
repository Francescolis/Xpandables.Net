
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
using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Operations;

namespace Xpandables.Net.Validators;

/// <summary>
/// A marker interface that allows the command/query/request class to be decorated 
/// with the validation behavior according to the class type.
/// </summary>
public interface IValidateDecorator
{
    /// <summary>
    /// Gets the current instance identifier.
    /// </summary>
    public Guid Id => Guid.NewGuid();
}

/// <summary>
/// Defines method contracts used to validate an argument using a decorator.
/// The implementation must be thread-safe when working 
/// in a multi-threaded environment.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Gets the zero-base order in which the validator will be executed.
    /// The default value is zero.
    /// </summary>
    public virtual int Order => 0;

    /// <summary>
    /// Validates the argument and returns validation state with errors if necessary.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="argument"/> is null.</exception>
    /// <exception cref="ValidationException">The 
    /// exception throws by the validator</exception>
    /// <returns>Returns a result state that contains validation information.</returns>
    public IOperationResult Validate(object argument)
        => OperationResults.Ok(argument).Build();

    /// <summary>
    /// Validates the argument and returns validation state with errors if necessary.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="argument"/> is null.</exception>
    /// <exception cref="ValidationException">The 
    /// exception thrown by the validator</exception>
    /// <returns>Returns a result state that contains validation information.</returns>
    public ValueTask<IOperationResult> ValidateAsync(object argument)
    {
        IOperationResult result = Validate(argument);
        return ValueTask.FromResult(result);
    }
}

/// <summary>
/// Defines method contracts used to validate 
/// a type-specific argument using a decorator.
/// The implementation must be thread-safe when 
/// working in a multi-threaded environment.
/// </summary>
/// <typeparam name="TArgument">Type of the argument to be validated.</typeparam>
public interface IValidator<in TArgument> : IValidator
    where TArgument : notnull
{
    /// <summary>
    /// Validates the argument and returns validation 
    /// state with errors if necessary.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="argument"/> is null.</exception>
    /// <exception cref="ValidationException">The 
    /// exception thrown by the validator</exception>
    /// <returns>Returns a result state 
    /// that contains validation information.</returns>
    public IOperationResult Validate(TArgument argument)
        => OperationResults.Ok().Build();

    IOperationResult IValidator.Validate(object argument)
        => Validate((TArgument)argument);

    /// <summary>
    /// Validates the argument and returns validation 
    /// state with errors if necessary.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="argument"/> is null.</exception>
    /// <exception cref="ValidationException">The 
    /// exception thrown by the validator</exception>
    /// <returns>Returns a result state that contains 
    /// validation information.</returns>
    public ValueTask<IOperationResult> ValidateAsync(TArgument argument)
    {
        IOperationResult result = Validate(argument);
        return ValueTask.FromResult(result);
    }

    ValueTask<IOperationResult> IValidator.ValidateAsync(object argument)
        => ValidateAsync((TArgument)argument);
}