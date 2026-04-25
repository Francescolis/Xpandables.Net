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
/// Defines a contract for accessing SQL data services, providing builders and mappers for constructing and mapping SQL
/// queries.
/// </summary>
/// <remarks>Implementations of this interface typically provide access to components that facilitate the creation
/// and mapping of SQL statements within a data access layer. This interface is intended to abstract the underlying
/// details of SQL query construction and result mapping, enabling flexible integration with various data sources or
/// ORMs.</remarks>
public interface IDataSqlServiceAccessor
{
	/// <summary>
	/// Gets the SQL builder used to construct data queries for the underlying data source.
	/// </summary>
	IDataSqlBuilder DataSqlBuilder { get; }

	/// <summary>
	/// Gets the SQL data mapper used for mapping between database records and application objects.
	/// </summary>
	IDataSqlMapper DataSqlMapper { get; }
}
