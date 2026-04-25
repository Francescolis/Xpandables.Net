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
/// Defines a factory for creating instances of data SQL service accessors based on a specified provider or SQL dialect.
/// </summary>
/// <remarks>Use this interface to obtain an appropriate data SQL service accessor for a given database provider
/// or SQL dialect. Implementations are responsible for returning an accessor that is compatible with the requested
/// provider or dialect.</remarks>
public interface IDataSqlServiceAccessorFactory
{
	/// <summary>
	/// Creates a new instance of an accessor for SQL data services based on the specified provider invariant name.
	/// </summary>
	/// <param name="providerInvariantName">The provider invariant name that identifies the database provider for which the accessor is created.</param>
	/// <returns>An instance of an object that provides access to SQL data services configured for the specified provider.</returns>
	IDataSqlServiceAccessor Create(string providerInvariantName);

	/// <summary>
	/// Creates a new instance of an accessor for SQL data services using the specified SQL dialect.
	/// </summary>
	/// <param name="sqlDialect">The SQL dialect to use for the data service accessor. Determines the SQL syntax and behavior supported by the
	/// accessor.</param>
	/// <returns>An instance of an object that provides access to SQL data services configured for the specified dialect.</returns>
	IDataSqlServiceAccessor Create(SqlDialect sqlDialect);
}
