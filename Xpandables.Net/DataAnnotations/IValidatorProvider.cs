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
    /// Returns a validator for the specified type.
    /// </summary>
    /// <param name="type">The type to get a validator for.</param>
    /// <returns>The validator for the specified type.</returns>
    IValidator? GetValidator(Type type);

    /// <summary>
    /// Returns a validator for the specified type.
    /// </summary>
    /// <typeparam name="TArgument">The type to get a validator for.</typeparam>
    /// <returns>The validator for the specified type.</returns>
    IValidator? GetValidator<TArgument>();
}