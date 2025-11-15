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
namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Provides a mechanism for retrieving validators for types that require validation.
/// </summary>
/// <remarks>This class is typically used to obtain an appropriate validator for a given type that implements the
/// IRequiresValidation interface. If a service provider is supplied, it is used to resolve validators from the
/// dependency injection container; otherwise, default validator instances are created at runtime. This class is sealed
/// and cannot be inherited.</remarks>
public sealed class ValidatorProvider(IValidatorFactory validatorFactory) : IValidatorProvider
{
    private readonly IValidatorFactory _validatorFactory = validatorFactory;

    /// <inheritdoc/>
    public IValidator? TryGetValidator(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        if (!typeof(IRequiresValidation).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type '{type.FullName}' must implement '{nameof(IRequiresValidation)}'.", nameof(type));
        }

        return _validatorFactory.CreateValidator(type);
    }

    /// <inheritdoc/>
    public IValidator? TryGetValidator<TArgument>()
        where TArgument : class, IRequiresValidation
    {
        return _validatorFactory.CreateValidator<TArgument>();
    }
}