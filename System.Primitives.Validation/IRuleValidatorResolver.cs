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
namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Defines a mechanism for resolving validators for a specific target type at runtime.
/// This is used to obtain validator instances dynamically, often in conjunction with dependency injection without 
/// relying on compile-time type information.
/// </summary>
/// <remarks>Implementations of this interface allow dynamic retrieval of validators, typically based on
/// dependency injection or service location. This is useful in scenarios where validation logic needs to be decoupled
/// from object creation or where multiple validator implementations may exist for different types.</remarks>
public interface IRuleValidatorResolver
{
    /// <summary>
    /// Gets the type of object that this instance targets.
    /// </summary>
    Type TargetType { get; }

    /// <summary>
    /// Resolves an instance of an <see cref="IRuleValidator"/> using the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to obtain the <see cref="IRuleValidator"/> instance. Cannot be null.</param>
    /// <returns>An <see cref="IRuleValidator"/> instance resolved from the service provider.</returns>
    IRuleValidator? Resolve(IServiceProvider serviceProvider);
}
