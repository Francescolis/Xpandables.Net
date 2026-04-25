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

internal sealed class DataSqlServiceAccessorFactory(IDataSqlMapper sqlMapper, IDataSqlBuilderFactory sqlBuilderFactory) : IDataSqlServiceAccessorFactory
{
	public IDataSqlServiceAccessor Create(string providerInvariantName)
	{
		ArgumentNullException.ThrowIfNull(providerInvariantName);
		IDataSqlBuilder sqlBuilder = sqlBuilderFactory.Create(providerInvariantName);
		return new DataSqlServiceAccessor(sqlBuilder, sqlMapper);
	}

	public IDataSqlServiceAccessor Create(SqlDialect sqlDialect)
	{
		IDataSqlBuilder sqlBuilder = sqlBuilderFactory.Create(sqlDialect);
		return new DataSqlServiceAccessor(sqlBuilder, sqlMapper);
	}
}
