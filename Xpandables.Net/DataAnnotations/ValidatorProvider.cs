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
using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Provides a way to get the validator for a given type.
/// </summary>
/// <param name="serviceProvider">The service provider to use.</param>
public sealed class ValidatorProvider(IServiceProvider serviceProvider) : IValidatorProvider
{
    private static readonly ConcurrentDictionary<Type, IValidator> _validators = new();
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private static readonly PeriodicTimer _cacheClearTimer = new(TimeSpan.FromHours(1));
#pragma warning disable CA1823 // Avoid unused private fields
    private static readonly Task _cacheClearTask = ClearCachePeriodicallyAsync();
#pragma warning restore CA1823 // Avoid unused private fields

    private static async Task ClearCachePeriodicallyAsync()
    {
        while (await _cacheClearTimer.WaitForNextTickAsync().ConfigureAwait(false))
        {
            _validators.Clear();
        }
    }

    /// <inheritdoc/>
    public IValidator? GetValidator(Type type)
    {
        if (_validators.TryGetValue(type, out var validator))
        {
            return validator;
        }

        validator = GetValidatorCore(type);

        if (validator is not null)
        {
            _validators.TryAdd(type, validator);
        }

        return validator;
    }

    /// <inheritdoc/>
    public IValidator? GetValidator<TArgument>() => GetValidator(typeof(TArgument));

    private IValidator? GetValidatorCore(Type type)
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
            var builtinType = typeof(Validator<>).MakeGenericType(type);
            validators = [.. validators.Where(validator => validator.GetType() != builtinType)];
        }

        return validators.FirstOrDefault();
    }
}
