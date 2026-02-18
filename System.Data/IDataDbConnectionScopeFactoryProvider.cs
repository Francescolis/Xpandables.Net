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
/// Defines methods for obtaining and creating database connection scope factories and scopes based on connection names
/// and optional provider overrides.
/// </summary>
/// <remarks>Implementations of this interface enable flexible management of database connection scopes,
/// supporting both synchronous and asynchronous operations. Callers can specify a connection name and, optionally, a
/// provider invariant name to control which database provider is used. This interface is intended to abstract the
/// creation and retrieval of connection scopes, allowing consumers to manage database lifetimes and contexts in a
/// consistent manner.</remarks>
public interface IDataDbConnectionScopeFactoryProvider
{
	/// <summary>
	/// Gets the scope factory for the specified connection name.
	/// </summary>
	/// <param name="name">The name of the connection.</param>
	IDataDbConnectionScopeFactory GetScopeFactory(string name);

	/// <summary>
	/// Gets the scope factory for the specified connection name and provider override.
	/// </summary>
	/// <param name="name">The name of the connection.</param>
	/// <param name="providerInvariantName">The provider invariant name override.</param>
	IDataDbConnectionScopeFactory GetScopeFactory(string name, string providerInvariantName);

	/// <summary>
	/// Creates a new scope for the specified connection name.
	/// </summary>
	/// <param name="name">The name of the connection.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<IDataDbConnectionScope> CreateScopeAsync(string name, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new scope for the specified connection name and provider override.
	/// </summary>
	/// <param name="name">The name of the connection.</param>
	/// <param name="providerInvariantName">The provider invariant name override.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<IDataDbConnectionScope> CreateScopeAsync(string name, string providerInvariantName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new scope for the specified connection name.
	/// </summary>
	/// <param name="name">The name of the connection.</param>
	IDataDbConnectionScope CreateScope(string name);

	/// <summary>
	/// Creates a new scope for the specified connection name and provider override.
	/// </summary>
	/// <param name="name">The name of the connection.</param>
	/// <param name="providerInvariantName">The provider invariant name override.</param>
	IDataDbConnectionScope CreateScope(string name, string providerInvariantName);
}
