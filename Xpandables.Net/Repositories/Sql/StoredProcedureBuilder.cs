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
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Xpandables.Net.Repositories.Sql;

/// <summary>
/// Provides a fluent interface for building and executing stored procedures with comprehensive parameter support and advanced features.
/// </summary>
public sealed class StoredProcedureBuilder
{
    private readonly string _procedureName;
    private readonly List<StoredProcedureParameter> _parameters = [];
    private TimeSpan? _timeout;
    private RetryPolicy? _retryPolicy;
    private DbConnection? _connection;
    private DbTransaction? _transaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="StoredProcedureBuilder"/> class.
    /// </summary>
    /// <param name="procedureName">The name of the stored procedure to execute.</param>
    internal StoredProcedureBuilder(string procedureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procedureName);
        _procedureName = procedureName;
    }

    /// <summary>
    /// Adds an input parameter to the stored procedure.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="dbType">The SQL database type (optional, will be inferred if not specified).</param>
    /// <param name="size">The parameter size (optional).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithInputParameter<T>(string name, T value, SqlDbType? dbType = null, int? size = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        _parameters.Add(StoredProcedureParameter.CreateInput(name, value, dbType, size));
        return this;
    }

    /// <summary>
    /// Adds an output parameter to the stored procedure.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="dbType">The SQL database type.</param>
    /// <param name="size">The parameter size (optional).</param>
    /// <param name="precision">The parameter precision (optional).</param>
    /// <param name="scale">The parameter scale (optional).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithOutputParameter(string name, SqlDbType dbType, int? size = null, byte? precision = null, byte? scale = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        _parameters.Add(StoredProcedureParameter.CreateOutput(name, dbType, size, precision, scale));
        return this;
    }

    /// <summary>
    /// Adds an input/output parameter to the stored procedure.
    /// </summary>
    /// <typeparam name="T">The type of the parameter value.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The initial parameter value.</param>
    /// <param name="dbType">The SQL database type.</param>
    /// <param name="size">The parameter size (optional).</param>
    /// <param name="precision">The parameter precision (optional).</param>
    /// <param name="scale">The parameter scale (optional).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithInputOutputParameter<T>(string name, T value, SqlDbType dbType, int? size = null, byte? precision = null, byte? scale = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        _parameters.Add(StoredProcedureParameter.CreateInputOutput(name, value, dbType, size, precision, scale));
        return this;
    }

    /// <summary>
    /// Adds a return value parameter to the stored procedure.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithReturnValue()
    {
        // Remove existing return value parameter if present
        _parameters.RemoveAll(p => p.Direction == ParameterDirection.ReturnValue);
        _parameters.Add(StoredProcedureParameter.CreateReturnValue());
        return this;
    }

    /// <summary>
    /// Adds a table-valued parameter to the stored procedure.
    /// </summary>
    /// <typeparam name="T">The type of the table data elements.</typeparam>
    /// <param name="name">The parameter name.</param>
    /// <param name="values">The table data values.</param>
    /// <param name="typeName">The user-defined table type name.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithTableParameter<T>(string name, IEnumerable<T> values, string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(values);

        // Convert to DataTable for SQL Server
        DataTable dataTable = CreateDataTableFromEnumerable(values);
        _parameters.Add(StoredProcedureParameter.CreateTableValued(name, dataTable, typeName));
        return this;
    }

    /// <summary>
    /// Adds a structured parameter using a pre-built DataTable.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="table">The data table containing the structured data.</param>
    /// <param name="typeName">The user-defined table type name.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithStructuredParameter(string name, DataTable table, string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(table);

        _parameters.Add(StoredProcedureParameter.CreateTableValued(name, table, typeName));
        return this;
    }

    /// <summary>
    /// Adds parameters based on an object's properties.
    /// </summary>
    /// <typeparam name="T">The type of the parameter object.</typeparam>
    /// <param name="parameters">The object containing parameter values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithParameters<T>(T parameters) where T : class
    {
        ArgumentNullException.ThrowIfNull(parameters);

        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (PropertyInfo property in properties)
        {
            if (property.CanRead)
            {
                object? value = property.GetValue(parameters);
                _parameters.Add(StoredProcedureParameter.CreateInput(property.Name, value));
            }
        }

        return this;
    }

    /// <summary>
    /// Adds parameters based on an expression that creates an anonymous object.
    /// </summary>
    /// <typeparam name="T">The type of the anonymous object.</typeparam>
    /// <param name="parameterExpression">The expression that creates the parameter object.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithParametersFromExpression<T>(Expression<Func<T>> parameterExpression)
    {
        ArgumentNullException.ThrowIfNull(parameterExpression);

        if (parameterExpression.Body is not NewExpression newExpression)
        {
            throw new ArgumentException("Expression must be a constructor call (new { ... })", nameof(parameterExpression));
        }

        for (int i = 0; i < newExpression.Arguments.Count; i++)
        {
            string? paramName = newExpression.Members?[i].Name;
            if (!string.IsNullOrEmpty(paramName))
            {
                // Evaluate the expression to get the value
                object? value = Expression.Lambda(newExpression.Arguments[i]).Compile().DynamicInvoke();
                _parameters.Add(StoredProcedureParameter.CreateInput(paramName, value));
            }
        }

        return this;
    }

    /// <summary>
    /// Sets the timeout for the stored procedure execution.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithTimeout(TimeSpan timeout)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(timeout.TotalMilliseconds);
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the retry policy for the stored procedure execution.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <param name="delay">The delay between retry attempts.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithRetryPolicy(int maxRetries, TimeSpan delay)
    {
        _retryPolicy = new RetryPolicy(maxRetries, delay);
        return this;
    }

    /// <summary>
    /// Sets the connection to use for the stored procedure execution.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithConnection(DbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _connection = connection;
        return this;
    }

    /// <summary>
    /// Sets the transaction to use for the stored procedure execution.
    /// </summary>
    /// <param name="transaction">The database transaction.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StoredProcedureBuilder WithTransaction(DbTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        _transaction = transaction;
        return this;
    }

    /// <summary>
    /// Executes the stored procedure asynchronously and returns comprehensive result information.
    /// </summary>
    /// <param name="connection">The database connection to use if none was previously set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A comprehensive result containing all execution details.</returns>
    public async Task<StoredProcedureResult> ExecuteAsync(DbConnection? connection = null, CancellationToken cancellationToken = default)
    {
        DbConnection connectionToUse = connection ?? _connection ?? throw new InvalidOperationException("No connection specified for stored procedure execution.");

        Func<Task<StoredProcedureResult>> operation = () => ExecuteInternalAsync(connectionToUse, cancellationToken);

        if (_retryPolicy is not null)
        {
            return await _retryPolicy.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
        }

        return await operation().ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the stored procedure and returns a scalar value.
    /// </summary>
    /// <typeparam name="T">The expected type of the scalar result.</typeparam>
    /// <param name="connection">The database connection to use if none was previously set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scalar result value.</returns>
    public async Task<T?> ExecuteScalarAsync<T>(DbConnection? connection = null, CancellationToken cancellationToken = default)
    {
        DbConnection connectionToUse = connection ?? _connection ?? throw new InvalidOperationException("No connection specified for stored procedure execution.");

        Func<Task<T?>> operation = () => ExecuteScalarInternalAsync<T>(connectionToUse, cancellationToken);

        if (_retryPolicy is not null)
        {
            return await _retryPolicy.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
        }

        return await operation().ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the stored procedure and returns a DataSet.
    /// </summary>
    /// <param name="connection">The database connection to use if none was previously set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A DataSet containing the result data.</returns>
    public async Task<DataSet> ExecuteDataSetAsync(DbConnection? connection = null, CancellationToken cancellationToken = default)
    {
        StoredProcedureResult result = await ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
        
        if (result.HasErrors)
        {
            throw new InvalidOperationException($"Stored procedure execution failed: {string.Join(", ", result.ErrorMessages)}");
        }

        return result.DataSet ?? new DataSet();
    }

    /// <summary>
    /// Executes the stored procedure as a non-query operation.
    /// </summary>
    /// <param name="connection">The database connection to use if none was previously set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> ExecuteNonQueryAsync(DbConnection? connection = null, CancellationToken cancellationToken = default)
    {
        StoredProcedureResult result = await ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
        
        if (result.HasErrors)
        {
            throw new InvalidOperationException($"Stored procedure execution failed: {string.Join(", ", result.ErrorMessages)}");
        }

        return result.RowsAffected;
    }

    /// <summary>
    /// Internal method that performs the actual stored procedure execution.
    /// </summary>
    private async Task<StoredProcedureResult> ExecuteInternalAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool ownsConnection = connection.State == ConnectionState.Closed;

        try
        {
            if (ownsConnection)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using DbCommand command = CreateCommand(connection);
            using DbDataAdapter adapter = CreateDataAdapter(command);
            
            DataSet dataSet = new();
            int rowsAffected = adapter.Fill(dataSet);

            // Extract output parameters
            Dictionary<string, object?> outputParams = ExtractOutputParameters(command);
            object? returnValue = ExtractReturnValue(command);

            stopwatch.Stop();
            return StoredProcedureResult.Success(outputParams, returnValue, dataSet, rowsAffected, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return StoredProcedureResult.Failure([ex.Message], stopwatch.Elapsed);
        }
        finally
        {
            if (ownsConnection && connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Internal method that performs scalar execution.
    /// </summary>
    private async Task<T?> ExecuteScalarInternalAsync<T>(DbConnection connection, CancellationToken cancellationToken)
    {
        bool ownsConnection = connection.State == ConnectionState.Closed;

        try
        {
            if (ownsConnection)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using DbCommand command = CreateCommand(connection);
            object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            if (result is T typedResult)
                return typedResult;

            if (result is not null)
            {
                try
                {
                    return (T)Convert.ChangeType(result, typeof(T));
                }
                catch (InvalidCastException)
                {
                    // Return default if conversion fails
                }
            }

            return default;
        }
        finally
        {
            if (ownsConnection && connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Creates the database command with all configured parameters.
    /// </summary>
    private DbCommand CreateCommand(DbConnection connection)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText = _procedureName;
        command.CommandType = CommandType.StoredProcedure;
        command.Transaction = _transaction;

        if (_timeout.HasValue)
        {
            command.CommandTimeout = (int)_timeout.Value.TotalSeconds;
        }

        // Add parameters
        foreach (StoredProcedureParameter param in _parameters)
        {
            DbParameter dbParam = command.CreateParameter();
            dbParam.ParameterName = param.Name;
            dbParam.Value = param.Value ?? DBNull.Value;
            dbParam.Direction = param.Direction;
            
            // Set database-specific properties if supported
            if (dbParam is IDbDataParameter dataParam)
            {
                dataParam.DbType = ConvertSqlDbTypeToDbType(param.DbType);
                
                if (param.Size.HasValue)
                    dataParam.Size = param.Size.Value;
                
                if (param.Precision.HasValue)
                    dataParam.Precision = param.Precision.Value;
                
                if (param.Scale.HasValue)
                    dataParam.Scale = param.Scale.Value;
            }

            command.Parameters.Add(dbParam);
        }

        return command;
    }

    /// <summary>
    /// Creates a data adapter for the given command.
    /// </summary>
    private static DbDataAdapter CreateDataAdapter(DbCommand command)
    {
        // This is a simplified implementation. In practice, you might need
        // provider-specific adapters for better performance
        return command.Connection?.GetType().Name switch
        {
            _ => new GenericDataAdapter(command)
        };
    }

    /// <summary>
    /// Extracts output parameter values from the executed command.
    /// </summary>
    private static Dictionary<string, object?> ExtractOutputParameters(DbCommand command)
    {
        Dictionary<string, object?> outputParams = [];

        foreach (DbParameter param in command.Parameters)
        {
            if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
            {
                outputParams[param.ParameterName] = param.Value == DBNull.Value ? null : param.Value;
            }
        }

        return outputParams;
    }

    /// <summary>
    /// Extracts the return value from the executed command.
    /// </summary>
    private static object? ExtractReturnValue(DbCommand command)
    {
        DbParameter? returnParam = command.Parameters
            .Cast<DbParameter>()
            .FirstOrDefault(p => p.Direction == ParameterDirection.ReturnValue);

        return returnParam?.Value == DBNull.Value ? null : returnParam?.Value;
    }

    /// <summary>
    /// Converts SqlDbType to DbType for generic database operations.
    /// </summary>
    private static DbType ConvertSqlDbTypeToDbType(SqlDbType sqlDbType)
    {
        return sqlDbType switch
        {
            SqlDbType.BigInt => DbType.Int64,
            SqlDbType.Binary => DbType.Binary,
            SqlDbType.Bit => DbType.Boolean,
            SqlDbType.Char => DbType.AnsiStringFixedLength,
            SqlDbType.DateTime => DbType.DateTime,
            SqlDbType.DateTime2 => DbType.DateTime2,
            SqlDbType.DateTimeOffset => DbType.DateTimeOffset,
            SqlDbType.Decimal => DbType.Decimal,
            SqlDbType.Float => DbType.Double,
            SqlDbType.Image => DbType.Binary,
            SqlDbType.Int => DbType.Int32,
            SqlDbType.Money => DbType.Currency,
            SqlDbType.NChar => DbType.StringFixedLength,
            SqlDbType.NText => DbType.String,
            SqlDbType.NVarChar => DbType.String,
            SqlDbType.Real => DbType.Single,
            SqlDbType.SmallDateTime => DbType.DateTime,
            SqlDbType.SmallInt => DbType.Int16,
            SqlDbType.SmallMoney => DbType.Currency,
            SqlDbType.Text => DbType.AnsiString,
            SqlDbType.Time => DbType.Time,
            SqlDbType.Timestamp => DbType.Binary,
            SqlDbType.TinyInt => DbType.Byte,
            SqlDbType.UniqueIdentifier => DbType.Guid,
            SqlDbType.VarBinary => DbType.Binary,
            SqlDbType.VarChar => DbType.AnsiString,
            SqlDbType.Variant => DbType.Object,
            SqlDbType.Xml => DbType.Xml,
            SqlDbType.Date => DbType.Date,
            SqlDbType.Structured => DbType.Object,
            _ => DbType.String
        };
    }

    /// <summary>
    /// Creates a DataTable from an enumerable of objects.
    /// </summary>
    private static DataTable CreateDataTableFromEnumerable<T>(IEnumerable<T> values)
    {
        DataTable table = new();
        
        if (!values.Any())
            return table;

        // Get properties of T
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        // Add columns
        foreach (PropertyInfo prop in properties)
        {
            Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            table.Columns.Add(prop.Name, propType);
        }

        // Add rows
        foreach (T item in values)
        {
            object[] values2 = properties.Select(p => p.GetValue(item) ?? DBNull.Value).ToArray();
            table.Rows.Add(values2);
        }

        return table;
    }
}