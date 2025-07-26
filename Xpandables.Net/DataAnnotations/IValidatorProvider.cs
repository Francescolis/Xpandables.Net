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
namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Provides methods to get a validator for a given type.
/// </summary>
public interface IValidatorProvider
{
    /// <summary>
    /// Attempts to retrieve a validator for the specified type.
    /// </summary>
    /// <param name="type">The type for which to retrieve the validator. Cannot be <see langword="null"/>.</param>
    /// <returns>An instance of <see cref="IValidator"/> if a validator for the specified type exists;  otherwise, <see
    /// langword="null"/>.</returns>
    IValidator? TryGetValidator(Type type);

    /// <summary>
    /// Attempts to retrieve a validator for the specified argument type.
    /// </summary>
    /// <typeparam name="TArgument">The type of the argument for which the validator is requested. Must be a reference type and implement <see
    /// cref="IRequiresValidation"/>.</typeparam>
    /// <returns>An instance of <see cref="IValidator"/> if a validator for <typeparamref name="TArgument"/> is available; 
    /// otherwise, <see langword="null"/>.</returns>
    IValidator? TryGetValidator<TArgument>()
        where TArgument : class, IRequiresValidation;
}