namespace Xpandables.Net.Sql;

/// <summary>
/// Defines the contract for stored procedure SQL builders.
/// </summary>
public interface IStoredProcedureSqlBuilder : ISqlBuilder
{
    /// <summary>
    /// Adds a parameter to the stored procedure call.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>A stored procedure SQL builder for method chaining.</returns>
    IStoredProcedureSqlBuilder AddParameter(string name, object? value);
}