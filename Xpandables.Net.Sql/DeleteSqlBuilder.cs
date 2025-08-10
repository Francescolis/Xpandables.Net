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

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Xpandables.Net.Sql;

/// <summary>
/// Implementation of DELETE SQL builder with fluent API.
/// </summary>
/// <typeparam name="TEntity">The entity type to delete.</typeparam>
internal sealed partial class DeleteSqlBuilder<TEntity> : IDeleteSqlBuilder<TEntity> where TEntity : class
{
    private readonly List<string> _whereClauses = [];
    private readonly SqlExpressionVisitor _expressionVisitor = new();

    public DeleteSqlBuilder()
    {
    }

    public IDeleteSqlBuilder<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _expressionVisitor.RegisterParameterAliases(predicate);
        var sql = _expressionVisitor.VisitAndGenerateSql(predicate.Body);
        if (sql.StartsWith('('))
        {
            sql = sql[1..^1]; // Remove outer parentheses
        }
        _whereClauses.Add($"({RemoveAliasFromWhereClause(sql)})");
        return this;
    }

    public SqlQueryResult Build()
    {
        var sql = new StringBuilder();
        var tableName = GetTableName<TEntity>();

        sql.Append(CultureInfo.InvariantCulture, $"DELETE FROM [{tableName}]");

        if (_whereClauses.Count > 0)
        {
            sql.AppendLine();
            sql.Append(CultureInfo.InvariantCulture, $"WHERE {string.Join(" AND ", _whereClauses)}");
        }

        return new SqlQueryResult(sql.ToString(), _expressionVisitor.Parameters);
    }

    private static string RemoveAliasFromWhereClause(string sql) =>
        // Removes patterns like [alias].[Column] => [Column]
        // Handles multiple aliases and nested brackets
        RemoveAliasRegex().Replace(sql, "[$1]");

    private static string GetTableName<T>()
    {
        var type = typeof(T);
        var tableAttribute = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\[[a-zA-Z0-9_]+\]\.\[([a-zA-Z0-9_]+)\]")]
    private static partial System.Text.RegularExpressions.Regex RemoveAliasRegex();
}