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
namespace Xpandables.Net.Executions.Deciders;

/// <summary>
/// Provides a mechanism to get providers for a dependency.
/// </summary>
public interface IDeciderDependencyManager
{
    /// <summary>
    /// Returns the dependency provider for the specified dependency type.
    /// </summary>
    /// <param name="dependencyType">The dependency type.</param>
    /// <returns>The dependency provider for the specified dependency type.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    IDeciderDependencyProvider GetDependencyProvider(Type dependencyType);
}

internal sealed class DeciderDependencyManager : IDeciderDependencyManager
{
    private readonly HashSet<IDeciderDependencyProvider> _dependencyProviders = [];
    public DeciderDependencyManager(IEnumerable<IDeciderDependencyProvider> dependencyProviders)
    {
        foreach (var dependencyProvider in dependencyProviders)
        {
            if (_dependencyProviders.Contains(dependencyProvider))
            {
                throw new InvalidOperationException(
                    $"The dependency provider for the type {dependencyProvider.GetType().Name} is already registered.");
            }

            _dependencyProviders.Add(dependencyProvider);
        }
    }
    public IDeciderDependencyProvider GetDependencyProvider(Type dependencyType) =>
        _dependencyProviders.FirstOrDefault(provider => provider.CanProvideDependency(dependencyType))
            ?? throw new InvalidOperationException(
                $"The dependency provider for the type {dependencyType.Name} is not registered.");
}