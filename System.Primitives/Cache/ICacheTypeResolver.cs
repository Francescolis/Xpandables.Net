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
namespace System.Cache;

/// <summary>
/// Defines a mechanism for resolving .NET types from their string representations, typically for use in caching
/// scenarios.
/// </summary>
/// <remarks>Implementations of this interface allow mapping type names to their corresponding <see cref="Type"/>
/// objects, which can be useful for serialization, deserialization, or cache key generation. The behavior when a type
/// name cannot be resolved differs between methods: <see cref="Resolve"/> throws an exception, while <see
/// cref="TryResolve"/> returns <see langword="null"/>.</remarks>
public interface ICacheTypeResolver
{
    /// <summary>
    /// Resolves a type by its name and returns the corresponding <see cref="Type"/> object.
    /// </summary>
    /// <param name="typeName">The name of the type to resolve. Cannot be null or empty.</param>
    /// <returns>A <see cref="Type"/> object representing the resolved type, or <see langword="null"/> if the type cannot be
    /// found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the type cannot be resolved.</exception>
    Type Resolve(string typeName);

    /// <summary>
    /// Attempts to resolve a type by its name.
    /// </summary>
    /// <remarks>This method does not throw an exception if the type cannot be resolved. Use the return value
    /// to determine whether the resolution was successful.</remarks>
    /// <param name="typeName">The name of the type to resolve. Cannot be null or empty.</param>
    /// <returns>A <see cref="Type"/> object representing the resolved type if found; otherwise, <see langword="null"/>.</returns>
    Type? TryResolve(string typeName);
}
