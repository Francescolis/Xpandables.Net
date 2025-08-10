namespace Xpandables.Net.Sql;
/// <summary>
/// Main entry point for building SQL queries using a fluent API.
/// </summary>
public static class SqlBuilder
{
    /// <summary>
    /// Starts building a SELECT query with the specified table as the main source.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the main table.</typeparam>
    /// <param name="alias">Optional alias for the main table.</param>
    /// <returns>A SELECT SQL builder for method chaining.</returns>
    public static ISelectSqlBuilder<TEntity> From<TEntity>(string? alias = null) where TEntity : class
        => new SelectSqlBuilder<TEntity>(alias);

    /// <summary>
    /// Starts building an INSERT query for the specified table.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the table to insert into.</typeparam>
    /// <returns>An INSERT SQL builder for method chaining.</returns>
    public static IInsertSqlBuilder<TEntity> Insert<TEntity>() where TEntity : class
        => new InsertSqlBuilder<TEntity>();

    /// <summary>
    /// Starts building an UPDATE query for the specified table.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the table to update.</typeparam>
    /// <param name="alias">Optional alias for the table.</param>
    /// <returns>An UPDATE SQL builder for method chaining.</returns>
    public static IUpdateSqlBuilder<TEntity> Update<TEntity>(string? alias = null) where TEntity : class
        => new UpdateSqlBuilder<TEntity>(alias);

    /// <summary>
    /// Starts building a DELETE query for the specified table.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the table to delete from.</typeparam>
    /// <param name="alias">Optional alias for the table.</param>
    /// <returns>A DELETE SQL builder for method chaining.</returns>
    public static IDeleteSqlBuilder<TEntity> Delete<TEntity>(string? alias = null) where TEntity : class
        => new DeleteSqlBuilder<TEntity>(alias);

    /// <summary>
    /// Starts building a stored procedure call.
    /// </summary>
    /// <param name="procedureName">The name of the stored procedure.</param>
    /// <returns>A stored procedure SQL builder for method chaining.</returns>
    public static IStoredProcedureSqlBuilder StoredProcedure(string procedureName)
        => new StoredProcedureSqlBuilder(procedureName);
}