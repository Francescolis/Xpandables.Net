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

namespace System.Cache;

/// <summary>
/// Provides functionality to resolve .NET types by name, using a memory cache to optimize repeated lookups across
/// loaded assemblies.
/// </summary>
/// <remarks>This class filters out common system and Microsoft assemblies when searching for types, focusing on
/// application-specific assemblies. Type lookups are cached for 30 minutes to reduce repeated reflection overhead. This
/// class is thread-safe and intended for use in scenarios where type resolution by name is required, such as
/// deserialization or dynamic type loading.</remarks>
public sealed class CacheTypeResolver : Disposable, ICacheTypeResolver
{
    private static MemoryAwareCache<string, Type> _memoryCache = new(TimeSpan.Zero, TimeSpan.Zero);
    private static Assembly[] _assemblies = [];
    private static readonly string[] _legacyPrefixes =
        ["System.", "Microsoft.", "netstandard", "WindowsBase", "PresentationCore", "PresentationFramework", "Xpandables"];

    /// <summary>
    /// Initializes a new instance of the CacheTypeResolver class using the specified memory cache.
    /// </summary>
    /// <param name="memoryCache">An optional memory-aware cache to use for storing type mappings. 
    /// If null, a default cache is used.</param>
    public CacheTypeResolver(MemoryAwareCache<string, Type>? memoryCache = default)
    {
        if (memoryCache is not null)
        {
            _memoryCache = memoryCache;
        }
    }

    /// <summary>
    /// Registers the specified assemblies for type resolution. If no assemblies are provided, registers all assemblies
    /// in the current application domain except those with legacy prefixes.
    /// </summary>
    /// <remarks>This method replaces any previously registered assemblies. Assemblies with names starting
    /// with legacy prefixes are excluded when registering all assemblies from the application domain.</remarks>
    /// <param name="assemblies">An array of assemblies to register for type resolution. If the array is empty or not specified, all non-legacy
    /// assemblies in the current application domain are registered.</param>
    [RequiresUnreferencedCode("Uses reflection to load types from assemblies.")]
    public static void RegisterAssemblies(params Assembly[] assemblies)
    {
        RegisterAssemblies(_ => true, assemblies);
    }

    /// <summary>
    /// Registers types from the specified assemblies that match the given predicate. If no assemblies are provided, registers all assemblies
    /// in the current application domain except those with legacy prefixes.
    /// </summary>
    /// <remarks>This method uses reflection to enumerate exported types from the specified assemblies. Types
    /// that satisfy the predicate are registered in the internal cache. Avoid passing assemblies that may contain types
    /// with sensitive or unwanted side effects, as all matching types will be registered. This method requires
    /// unreferenced code and may not be compatible with trimming scenarios.</remarks>
    /// <param name="predicate">A delegate that defines the conditions each type must satisfy to be registered. The predicate is applied to each
    /// exported type in the provided assemblies.</param>
    /// <param name="assemblies">An array of assemblies from which types will be considered for registration. If no assemblies are specified, all
    /// currently loaded assemblies in the application domain are used.</param>
    [RequiresUnreferencedCode("Uses reflection to load types from assemblies.")]
    public static void RegisterAssemblies(Predicate<Type> predicate, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _assemblies = assemblies is { Length: > 0 }
            ? assemblies
            : [.. AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a =>
                {
                    var name = a.GetName().Name;
                    return name is not null
                        && !_legacyPrefixes.Any(prefix =>
                            name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                })];

        foreach (var assembly in _assemblies)
        {
            foreach (var type in assembly.ExportedTypes)
            {
                if (predicate(type))
                {
                    _memoryCache.GetOrAdd(type.Name, _ => type);
                }
            }
        }
    }

    /// <inheritdoc/>
    public Type Resolve(string typeName)
    {
        return TryResolve(typeName)
            ?? throw new InvalidOperationException($"The type '{typeName}' could not be resolved.");
    }

    /// <inheritdoc/>
    public Type? TryResolve(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        string key = typeName;
        return _memoryCache.TryGetValue(key, out Type? cachedType) ? cachedType : null;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memoryCache.Dispose();
        }

        base.Dispose(disposing);
    }
}
