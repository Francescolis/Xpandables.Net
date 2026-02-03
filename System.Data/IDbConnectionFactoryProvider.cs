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
using System.Diagnostics.CodeAnalysis;

namespace System.Data;

/// <summary>
/// Defines methods for retrieving database connection factories by connection name, with optional support for provider
/// overrides and safe retrieval patterns.
/// </summary>
/// <remarks>Implementations of this interface enable flexible management of database connection factories,
/// allowing consumers to obtain factories based on connection names and, if needed, override the default provider using
/// a provider invariant name. The interface supports both direct retrieval and safe attempts to obtain a factory
/// without throwing exceptions, facilitating robust and customizable database connectivity strategies.</remarks>
public interface IDbConnectionFactoryProvider
{
    /// <summary>
    /// Gets the connection factory for the specified connection name.
    /// </summary>
    /// <param name="name">The name of the connection.</param>
    IDbConnectionFactory GetFactory(string name);

    /// <summary>
    /// Gets the connection factory for the specified connection name and provider override.
    /// </summary>
    /// <param name="name">The name of the connection.</param>
    /// <param name="providerInvariantName">The provider invariant name to override the default provider.</param>
    IDbConnectionFactory GetFactory(string name, string providerInvariantName);

    /// <summary>
    /// Tries to get the connection factory for the specified connection name.
    /// </summary>
    /// <param name="name">The name of the connection.</param>
    /// <param name="factory">When this method returns, contains the connection factory associated with the specified name, if the name is found; otherwise, null.</param>
    bool TryGetFactory(string name, [NotNullWhen(true)] out IDbConnectionFactory? factory);

    /// <summary>
    /// Tries to get the connection factory for the specified connection name and provider override.
    /// </summary>
    /// <param name="name">The name of the connection.</param>
    /// <param name="providerInvariantName">The provider invariant name to override the default provider.</param>
    /// <param name="factory">When this method returns, contains the connection factory associated with the specified name and provider, if the name is found; otherwise, null.</param>
    bool TryGetFactory(string name, string providerInvariantName, [NotNullWhen(true)] out IDbConnectionFactory? factory);
}