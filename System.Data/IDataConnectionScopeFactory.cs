/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
/// Defines a factory for creating <see cref="IDataConnectionScope"/> instances.
/// </summary>
/// <remarks>
/// Use this factory to create scoped connections that manage their own lifecycle.
/// The factory is typically registered as a singleton in the dependency injection container.
/// </remarks>
public interface IDataConnectionScopeFactory
{
	/// <summary>
	/// Creates a new connection scope asynchronously with a closed connection.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>A task representing the asynchronous operation, containing the connection scope.</returns>
	Task<IDataConnectionScope> CreateScopeAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously creates and opens a new data connection scope.
	/// </summary>
	/// <remarks>The returned scope manages the lifetime of the underlying data connection. Disposing the scope will
	/// close the connection and release associated resources.</remarks>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an open data connection scope that must
	/// be disposed when no longer needed.</returns>
	Task<IDataConnectionScope> CreateOpenScopeAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new connection scope synchronously with a closed connection.
	/// </summary>
	/// <returns>The connection scope with a closed connection.</returns>
	IDataConnectionScope CreateScope();

	/// <summary>
	/// Creates and opens a new data connection scope for performing operations within a managed context.
	/// </summary>
	/// <remarks>The returned scope manages the lifetime of the underlying data connection. Disposing the scope will
	/// close the connection and release associated resources. This method is typically used to ensure that data operations
	/// are executed within a well-defined transactional or resource boundary.</remarks>
	/// <returns>An object that represents the opened data connection scope. The caller is responsible for disposing the returned
	/// scope when operations are complete.</returns>
	IDataConnectionScope CreateOpenScope();
}
