using System.Data;

namespace Xpandables.Net.Sql;
/// <summary>
/// Represents a SQL query result containing the parametrized SQL string and parameters.
/// </summary>
/// <param name="Sql">The parametrized SQL string.</param>
/// <param name="Parameters">The collection of SQL parameters.</param>
public readonly record struct SqlQueryResult(string Sql, IReadOnlyCollection<IDbDataParameter> Parameters);

/// <summary>
/// Defines the contract for SQL builders that can construct parametrized SQL queries.
/// </summary>
public interface ISqlBuilder
{
    /// <summary>
    /// Builds the SQL query and returns the parametrized SQL string with parameters.
    /// </summary>
    /// <returns>A <see cref="SqlQueryResult"/> containing the SQL string and parameters.</returns>
    SqlQueryResult Build();
}