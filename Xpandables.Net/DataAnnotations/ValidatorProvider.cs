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
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Provides a way to get the validator for a given type.
/// </summary>
/// <param name="serviceProvider">The service provider to use.</param>
public sealed class ValidatorProvider(IServiceProvider serviceProvider) : IValidatorProvider
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public IValidator? TryGetValidator(Type type)
    {
        Type validatorType = typeof(IValidator<>).MakeGenericType(type);

        var validators = _serviceProvider
            .GetServices(validatorType)
            .OfType<IValidator>()
            .ToList();

        if (validators.Count > 1)
        {
            // remove the built-in validator if a specific validator
            // is registered.
            var builtInValidator = typeof(Validator<>).MakeGenericType(type);
            validators = [.. validators.Where(validator => validator.GetType() != builtInValidator)];
        }

        return validators.FirstOrDefault();
    }

    /// <inheritdoc/>
    public IValidator? TryGetValidator<TArgument>()
        where TArgument : class, IValidationEnabled
    {
        var validators = _serviceProvider
            .GetServices<IValidator<TArgument>>()
            .ToList();

        if (validators.Count > 1)
        {
            // remove the built-in validator if a specific validator
            // is registered.
            var builtInValidatorType = typeof(Validator<TArgument>);
            validators = [.. validators.Where(validator => validator.GetType() != builtInValidatorType)];
        }

        return validators.FirstOrDefault();
    }
}