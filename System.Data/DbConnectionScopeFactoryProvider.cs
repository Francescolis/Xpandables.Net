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
namespace System.Data;

/// <summary>
/// Provides a factory for creating database connection scopes, utilizing an underlying IDbConnectionFactoryProvider.
/// </summary>
/// <remarks>This class is sealed and cannot be inherited. It ensures that the provided factory provider is not
/// null and facilitates the creation of connection scopes based on the specified name and provider invariant
/// name.</remarks>
/// <param name="factoryProvider">The factory provider used to create database connection factories. This parameter cannot be null.</param>
public sealed class DbConnectionScopeFactoryProvider(IDbConnectionFactoryProvider factoryProvider) : IDbConnectionScopeFactoryProvider
{
    private readonly IDbConnectionFactoryProvider _factoryProvider = factoryProvider ?? throw new ArgumentNullException(nameof(factoryProvider));

    /// <inheritdoc />
    public IDbConnectionScopeFactory GetScopeFactory(string name) =>
        new DbConnectionScopeFactory(_factoryProvider.GetFactory(name));

    /// <inheritdoc />
    public IDbConnectionScopeFactory GetScopeFactory(string name, string providerInvariantName) =>
        new DbConnectionScopeFactory(_factoryProvider.GetFactory(name, providerInvariantName));

    /// <inheritdoc />
    public Task<IDbConnectionScope> CreateScopeAsync(string name, CancellationToken cancellationToken = default) =>
        GetScopeFactory(name).CreateScopeAsync(cancellationToken);

    /// <inheritdoc />
    public Task<IDbConnectionScope> CreateScopeAsync(string name, string providerInvariantName, CancellationToken cancellationToken = default) =>
        GetScopeFactory(name, providerInvariantName).CreateScopeAsync(cancellationToken);

    /// <inheritdoc />
    public IDbConnectionScope CreateScope(string name) =>
        GetScopeFactory(name).CreateScope();

    /// <inheritdoc />
    public IDbConnectionScope CreateScope(string name, string providerInvariantName) =>
        GetScopeFactory(name, providerInvariantName).CreateScope();
}