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
using System.Diagnostics.CodeAnalysis;
using System.Net.Validators;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.Validators;

/// <summary>
/// Provides a mechanism for retrieving validators for types that require validation.
/// </summary>
/// <remarks>This class is typically used to obtain an appropriate validator for a given type that implements the
/// IRequiresValidation interface. If a service provider is supplied, it is used to resolve validators from the
/// dependency injection container; otherwise, default validator instances are created at runtime. This class is sealed
/// and cannot be inherited.</remarks>
/// <param name="serviceProvider">An optional service provider used to resolve validator instances. If null, default validators are created using
/// reflection.</param>
public sealed class ValidatorProvider(IServiceProvider? serviceProvider = null) : IValidatorProvider
{
    private readonly IServiceProvider? _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    [RequiresDynamicCode("The native code for an IEnumerable<serviceType> might not be available at runtime.")]
    public IValidator? TryGetValidator(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        if (!typeof(IRequiresValidation).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type '{type.FullName}' must implement '{typeof(IRequiresValidation).FullName}'.", nameof(type));
        }

        if (_serviceProvider is null)
        {
            Type defaultType = typeof(Validator<>).MakeGenericType(type);
            return Activator.CreateInstance(defaultType) as IValidator;
        }

        Type validatorType = typeof(IValidator<>).MakeGenericType(type);
        var validators = _serviceProvider.GetServices(validatorType).OfType<IValidator>().ToList();
        Type builtInValidatorType = typeof(Validator<>).MakeGenericType(type);
        return RemoveBuiltInValidatorIfExists(validators, builtInValidatorType);
    }

    /// <inheritdoc/>
    [RequiresDynamicCode("The native code for an IEnumerable<serviceType> might not be available at runtime.")]
    public IValidator? TryGetValidator<TArgument>()
        where TArgument : class, IRequiresValidation => TryGetValidator(typeof(TArgument));

    private static IValidator? RemoveBuiltInValidatorIfExists(List<IValidator> validators, Type builtInValidatorType)
    {
        if (validators.Count > 1)
        {
            // Remove the built-in validator if a specific validator is registered.
            validators = [.. validators.Where(validator => validator.GetType() != builtInValidatorType)];
        }

        return validators.FirstOrDefault();
    }
}