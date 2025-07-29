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
/// Provides functionality to retrieve validators for specific types.
/// </summary>
/// <remarks>This class is responsible for obtaining validators from the registered services. It supports
/// retrieving validators for both specific types and generic type arguments. If multiple validators are found, it
/// prioritizes custom validators over built-in ones.</remarks>
/// <param name="serviceProvider"></param>
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

        Type builtInValidatorType = typeof(Validator<>).MakeGenericType(type);
        return FilterAndGetValidator(validators, builtInValidatorType);
    }

    /// <inheritdoc/>
    public IValidator? TryGetValidator<TArgument>()
        where TArgument : class, IRequiresValidation
    {
        var validators = _serviceProvider
            .GetServices<IValidator<TArgument>>()
            .ToList();

        Type builtInValidatorType = typeof(Validator<TArgument>);
        return FilterAndGetValidator([.. validators.OfType<IValidator>()], builtInValidatorType);
    }

    private static IValidator? FilterAndGetValidator(List<IValidator> validators, Type builtInValidatorType)
    {
        if (validators.Count > 1)
        {
            // Remove the built-in validator if a specific validator is registered.
            validators = [.. validators.Where(validator => validator.GetType() != builtInValidatorType)];
        }

        return validators.FirstOrDefault();
    }
}