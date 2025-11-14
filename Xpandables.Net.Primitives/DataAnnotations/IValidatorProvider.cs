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
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Defines a mechanism for retrieving validators for specific types or arguments that require validation.
/// </summary>
/// <remarks>Implementations of this interface provide access to validators that can be used to validate objects
/// at runtime. This is typically used in scenarios where validation logic needs to be applied dynamically based on the
/// type of the object or argument.</remarks>
public interface IValidatorProvider
{
    /// <summary>
    /// Attempts to retrieve a validator for the specified type.
    /// </summary>
    /// <param name="type">The type for which to retrieve a validator. Cannot be null.</param>
    /// <returns>An instance of <see cref="IValidator"/> for the specified type if one is available; otherwise, <see
    /// langword="null"/>.</returns>
    [RequiresDynamicCode("The native code for Activator.CreateInstance might not be available at runtime.")]
    [RequiresUnreferencedCode("The validator type might be removed by the linker.")]
    IValidator? TryGetValidator(Type type);

    /// <summary>
    /// Attempts to retrieve a validator for the specified argument type.
    /// </summary>
    /// <typeparam name="TArgument">The type of argument for which to retrieve a validator. Must be a reference type that implements
    /// IRequiresValidation.</typeparam>
    /// <returns>An instance of IValidator for the specified argument type if one is available; otherwise, null.</returns>
    IValidator? TryGetValidator<TArgument>()
        where TArgument : class, IRequiresValidation;
}
