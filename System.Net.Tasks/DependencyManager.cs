
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
using System.Net.Tasks;

namespace System.Net.Tasks;

/// <summary>
/// Provides access to registered dependency providers and enables retrieval of providers capable of supplying specific
/// dependency types.
/// </summary>
/// <remarks>Use this class to manage and query a collection of dependency providers within an application. This
/// class is sealed and cannot be inherited.</remarks>
public sealed class DependencyManager : IDependencyManager
{
    private readonly HashSet<IDependencyProvider> _dependencyProviders = [];
    /// <summary>
    /// Initializes a new instance of the DependencyManager class using the specified collection of dependency
    /// providers.
    /// </summary>
    /// <remarks>Each provider in the collection is added to the manager and will be used to resolve
    /// dependencies. The order of providers may affect resolution if multiple providers can supply the same
    /// dependency.</remarks>
    /// <param name="dependencyProviders">A collection of dependency providers to be managed. Cannot be null.</param>
    public DependencyManager(IEnumerable<IDependencyProvider> dependencyProviders)
    {
        ArgumentNullException.ThrowIfNull(dependencyProviders);

        foreach (IDependencyProvider dependencyProvider in dependencyProviders)
        {
            _dependencyProviders.Add(dependencyProvider);
        }
    }

    /// <inheritdoc/>
    public IDependencyProvider GetDependencyProvider(Type dependencyType)
    {
        ArgumentNullException.ThrowIfNull(dependencyType);

        return _dependencyProviders.FirstOrDefault(provider => provider.CanProvideDependency(dependencyType))
            ?? throw new InvalidOperationException(
                $"The dependency provider for the type {dependencyType.Name} is not registered.");
    }
}