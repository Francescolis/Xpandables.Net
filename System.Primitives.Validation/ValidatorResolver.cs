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
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides a type-safe resolver for retrieving validators for a specific argument type that requires validation.
/// This is used to avoid reflection and improve performance when the argument type is known at compile time.
/// </summary>
/// <remarks>Use this class to obtain an <see cref="IValidator{TArgument}"/> instance from a service provider for
/// the specified argument type. This enables generic validation scenarios where the argument type is known at compile
/// time.</remarks>
/// <typeparam name="TArgument">The type of argument to be validated. Must be a reference type that implements <see cref="IRequiresValidation"/>.</typeparam>
public sealed class ValidatorResolver<TArgument> : IValidatorResolver
    where TArgument : class, IRequiresValidation
{
    /// <inheritdoc/>
    public Type TargetType => typeof(TArgument);

    /// <inheritdoc/>
    public IValidator? Resolve(IServiceProvider serviceProvider)
    {
        var validators = serviceProvider.GetServices<IValidator<TArgument>>().ToList();
        Type builtInValidatorType = typeof(DefaultValidator<TArgument>);
        if (validators.Count > 1)
        {
            // Remove the built-in validator if a specific validator is registered.
            validators = [.. validators.Where(validator => validator.GetType() != builtInValidatorType)];
        }

        return validators.FirstOrDefault();
    }
}
