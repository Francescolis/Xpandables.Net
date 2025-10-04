
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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace System.Net.Validators;

/// <summary>
/// Defines a contract for validating objects and returning validation results.
/// </summary>
/// <remarks>Implementations of this interface can be used to check whether an object meets specific validation
/// criteria. The interface provides both synchronous and asynchronous validation methods to support different usage
/// scenarios.</remarks>
public interface IValidator
{
    /// <summary>
    /// Validates the specified object instance and returns a collection of validation results.
    /// </summary>
    /// <param name="instance">The object to validate. Cannot be null.</param>
    /// <returns>A read-only collection of <see cref="ValidationResult"/> objects that describe any validation errors. The
    /// collection is empty if the instance is valid.</returns>
    [RequiresUnreferencedCode("Validation may not work correctly if the object graph is modified.")]
    IReadOnlyCollection<ValidationResult> Validate(object instance);

    /// <summary>
    /// Asynchronously validates the specified object instance and returns a collection of validation results.
    /// </summary>
    /// <remarks>This method executes validation synchronously and returns a completed <see
    /// cref="ValueTask{TResult}"/>. Use this method when an asynchronous signature is required, but the validation
    /// itself does not involve asynchronous operations.</remarks>
    /// <param name="instance">The object to validate. Cannot be null.</param>
    /// <returns>A value task that represents the asynchronous validation operation. The result contains a read-only collection
    /// of <see cref="ValidationResult"/> objects describing any validation errors. The collection is empty if the
    /// object is valid.</returns>
    [RequiresUnreferencedCode("Validation may not work correctly if the object graph is modified.")]
    public ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(object instance)
    {
        IReadOnlyCollection<ValidationResult> result = Validate(instance);
        return new ValueTask<IReadOnlyCollection<ValidationResult>>(result);
    }
}

/// <summary>
/// Defines a contract for validating objects of a specified type and returning validation results.
/// </summary>
/// <remarks>Implementations of this interface provide both synchronous and asynchronous validation methods. The
/// interface is typically used to enforce business rules or data integrity constraints on objects before further
/// processing. Implementers should ensure that validation logic is thread-safe if the validator will be used
/// concurrently.</remarks>
/// <typeparam name="TArgument">The type of object to validate. Must be a reference type that implements <see cref="IRequiresValidation"/>.</typeparam>
public interface IValidator<in TArgument> : IValidator
    where TArgument : class, IRequiresValidation
{
    /// <summary>
    /// Validates the specified instance and returns a collection of validation results.
    /// </summary>
    /// <param name="instance">The object to validate. Cannot be null.</param>
    /// <returns>A read-only collection of <see cref="ValidationResult"/> objects that describe any validation errors. The
    /// collection is empty if the instance is valid.</returns>
    [RequiresUnreferencedCode("Validation may not work correctly if the object graph is modified.")]
    IReadOnlyCollection<ValidationResult> Validate(TArgument instance);

    [RequiresUnreferencedCode("Validation may not work correctly if the object graph is modified.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    IReadOnlyCollection<ValidationResult> IValidator.Validate(object instance) =>
        Validate((TArgument)instance);

    /// <summary>
    /// Asynchronously validates the specified argument and returns a collection of validation results.
    /// </summary>
    /// <param name="instance">The argument to validate. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous validation operation. The task result contains a read-only collection of
    /// <see cref="ValidationResult"/> objects describing any validation errors. The collection is empty if the argument
    /// is valid.</returns>
    [RequiresUnreferencedCode("Validation may not work correctly if the object graph is modified.")]
    ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(TArgument instance);

    [RequiresUnreferencedCode("Validation may not work correctly if the object graph is modified.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    ValueTask<IReadOnlyCollection<ValidationResult>> IValidator.ValidateAsync(object instance) =>
        ValidateAsync((TArgument)instance);
}