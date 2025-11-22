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
/// Provides a factory for creating validator instances based on type information or generic argument. This class
/// enables dynamic resolution of validators using registered services and custom validator resolvers.
/// </summary>
/// <remarks>This factory supports both direct service-based resolution and custom type-based resolution through
/// the provided validator resolvers. It is intended for use in scenarios where validator types may be registered
/// dynamically or require custom resolution logic.</remarks>
/// <param name="serviceProvider">The service provider used to resolve validator instances and dependencies.</param>
/// <param name="validatorResolvers">A collection of validator resolvers that map specific types to their corresponding validators.</param>
public sealed class RuleValidatorFactory(
    IServiceProvider serviceProvider,
    IEnumerable<IRuleValidatorResolver> validatorResolvers) : IRuleValidatorFactory
{
    /// <inheritdoc/>
    public IRuleValidator? CreateValidator(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var resolver = validatorResolvers.FirstOrDefault(r => r.TargetType == type);
        return resolver?.Resolve(serviceProvider);
    }

    /// <inheritdoc/>
    IRuleValidator<TArgument>? IRuleValidatorFactory.CreateValidator<TArgument>()
    {
        return serviceProvider.GetService<IRuleValidator<TArgument>>();
    }
}
