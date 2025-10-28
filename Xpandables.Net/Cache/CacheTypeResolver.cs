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
using System.Reflection;

using Microsoft.Extensions.Caching.Memory;

namespace Xpandables.Net.Cache;

/// <summary>
/// Provides functionality to resolve .NET types by name, using a memory cache to optimize repeated lookups across
/// loaded assemblies.
/// </summary>
/// <remarks>This class filters out common system and Microsoft assemblies when searching for types, focusing on
/// application-specific assemblies. Type lookups are cached for 30 minutes to reduce repeated reflection overhead. This
/// class is thread-safe and intended for use in scenarios where type resolution by name is required, such as
/// deserialization or dynamic type loading.</remarks>
/// <remarks>
/// Initializes a new instance of the CacheTypeResolver class using the specified memory cache.
/// </remarks>
/// <param name="memoryCache">The memory cache instance used to store and retrieve type resolution data. Cannot be null.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="memoryCache"/> is null.</exception>
public sealed class CacheTypeResolver(IMemoryCache memoryCache) : ICacheTypeResolver
{
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    private Assembly[] _assemblies = [];
    private static readonly string[] _legacyPrefixes =
        ["System.", "Microsoft.", "netstandard", "WindowsBase", "PresentationCore", "PresentationFramework"];

    /// <summary>
    /// Registers the specified assemblies for type resolution. If no assemblies are provided, registers all assemblies
    /// in the current application domain except those with legacy prefixes.
    /// </summary>
    /// <remarks>This method replaces any previously registered assemblies. Assemblies with names starting
    /// with legacy prefixes are excluded when registering all assemblies from the application domain.</remarks>
    /// <param name="assemblies">An array of assemblies to register for type resolution. If the array is empty or not specified, all non-legacy
    /// assemblies in the current application domain are registered.</param>
    public void RegisterAssemblies(params Assembly[] assemblies)
    {
        _assemblies = assemblies is { Length: > 0 }
            ? assemblies
            : [.. AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !_legacyPrefixes
                    .Any(prefix => a.GetName().Name!
                        .StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase) == true))];
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection to load types from assemblies.")]
    public Type Resolve(string typeName)
    {
        return TryResolve(typeName)
            ?? throw new InvalidOperationException($"The type '{typeName}' could not be resolved.");
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection to load types from assemblies.")]
    public Type? TryResolve(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return _memoryCache.GetOrCreate(
            $"CacheTypeResolver_TryResolve_{typeName}",
            entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(30));
                return ResolveType(typeName);
            });
    }

    [RequiresUnreferencedCode("Uses reflection to load types from assemblies.")]
    private Type? ResolveType(string typeName)
    {
        return _assemblies
            .SelectMany(a => a.ExportedTypes)
            .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
    }
}