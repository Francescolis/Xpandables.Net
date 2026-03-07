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
/// Defines a factory for creating <see cref="IDataDbConnectionScope"/> instances.
/// </summary>
/// <remarks>
/// Use this factory to create scoped connections that manage their own lifecycle.
/// The factory is typically registered as a singleton in the dependency injection container.
/// </remarks>
public interface IDataDbConnectionScopeFactory
{
	/// <summary>
	/// Creates a new connection scope asynchronously with an open connection.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>A task representing the asynchronous operation, containing the connection scope.</returns>
	Task<IDataDbConnectionScope> CreateScopeAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new connection scope synchronously with an open connection.
	/// </summary>
	/// <returns>The connection scope with an open connection.</returns>
	IDataDbConnectionScope CreateScope();
}
