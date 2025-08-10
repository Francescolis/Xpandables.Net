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

using System.Data;
using System.Globalization;
using System.Text;

using Microsoft.Data.SqlClient;

namespace Xpandables.Net.Sql;

/// <summary>
/// Implementation of stored procedure SQL builder with fluent API.
/// </summary>
internal sealed class StoredProcedureSqlBuilder : IStoredProcedureSqlBuilder
{
    private readonly string _procedureName;
    private readonly List<IDbDataParameter> _parameters = [];

    public StoredProcedureSqlBuilder(string procedureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procedureName);
        _procedureName = procedureName;
    }

    public IStoredProcedureSqlBuilder AddParameter(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var parameterName = name.StartsWith('@') ? name : $"@{name}";
        _parameters.Add(new SqlParameter(parameterName, value ?? DBNull.Value));
        return this;
    }

    public IStoredProcedureSqlBuilder AddParameters(IDictionary<string, object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        foreach (var kvp in parameters)
        {
            AddParameter(kvp.Key, kvp.Value);
        }

        return this;
    }

    public SqlQueryResult Build()
    {
        var sql = new StringBuilder();
        sql.Append(CultureInfo.InvariantCulture, $"EXEC [{_procedureName}]");

        if (_parameters.Count > 0)
        {
            sql.Append(' ');
            sql.Append(string.Join(", ", _parameters.Select(p => p.ParameterName)));
        }

        return new SqlQueryResult(sql.ToString(), _parameters);
    }
}