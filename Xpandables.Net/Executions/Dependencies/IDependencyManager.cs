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
namespace Xpandables.Net.Executions.Dependencies;

/// <summary>
/// Provides methods to manage and retrieve dependency providers for specified types.
/// </summary>
/// <remarks>This interface defines the contract for managing dependencies within an application, allowing
/// retrieval of dependency providers based on the type of dependency required. Implementations should handle the
/// registration and resolution of dependencies.</remarks>
public interface IDependencyManager
{
    /// <summary>
    /// Returns the dependency provider for the specified dependency type.
    /// </summary>
    /// <param name="dependencyType">The dependency type.</param>
    /// <returns>The dependency provider for the specified dependency type.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    IDependencyProvider GetDependencyProvider(Type dependencyType);
}

internal sealed class DependencyManager : IDependencyManager
{
    private readonly HashSet<IDependencyProvider> _dependencyProviders = [];
    public DependencyManager(IEnumerable<IDependencyProvider> dependencyProviders)
    {
        foreach (IDependencyProvider dependencyProvider in dependencyProviders)
        {
            _dependencyProviders.Add(dependencyProvider);
        }
    }
    public IDependencyProvider GetDependencyProvider(Type dependencyType) =>
        _dependencyProviders.FirstOrDefault(provider => provider.CanProvideDependency(dependencyType))
            ?? throw new InvalidOperationException(
                $"The dependency provider for the type {dependencyType.Name} is not registered.");
}