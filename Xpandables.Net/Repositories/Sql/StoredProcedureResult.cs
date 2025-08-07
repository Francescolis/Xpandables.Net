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
using System.Collections.ObjectModel;

namespace Xpandables.Net.Repositories.Sql;

/// <summary>
/// Represents the result of a stored procedure execution with comprehensive result information.
/// </summary>
public sealed class StoredProcedureResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoredProcedureResult"/> class.
    /// </summary>
    /// <param name="outputParameters">The output parameter values.</param>
    /// <param name="returnValue">The return value from the stored procedure.</param>
    /// <param name="dataSet">The result data set.</param>
    /// <param name="rowsAffected">The number of rows affected.</param>
    /// <param name="executionTime">The execution time.</param>
    /// <param name="errorMessages">The error messages if any.</param>
    internal StoredProcedureResult(
        IDictionary<string, object?> outputParameters,
        object? returnValue,
        DataSet? dataSet,
        int rowsAffected,
        TimeSpan executionTime,
        IList<string>? errorMessages = null)
    {
        OutputParameters = new ReadOnlyDictionary<string, object?>(outputParameters ?? new Dictionary<string, object?>());
        ReturnValue = returnValue;
        DataSet = dataSet;
        RowsAffected = rowsAffected;
        ExecutionTime = executionTime;
        ErrorMessages = new ReadOnlyCollection<string>(errorMessages ?? Array.Empty<string>());
    }

    /// <summary>
    /// Gets the output parameter values from the stored procedure execution.
    /// </summary>
    public IReadOnlyDictionary<string, object?> OutputParameters { get; }

    /// <summary>
    /// Gets the return value from the stored procedure.
    /// </summary>
    public object? ReturnValue { get; }

    /// <summary>
    /// Gets the result data set from the stored procedure execution.
    /// </summary>
    public DataSet? DataSet { get; }

    /// <summary>
    /// Gets the number of rows affected by the stored procedure execution.
    /// </summary>
    public int RowsAffected { get; }

    /// <summary>
    /// Gets the execution time of the stored procedure.
    /// </summary>
    public TimeSpan ExecutionTime { get; }

    /// <summary>
    /// Gets a value indicating whether the execution had errors.
    /// </summary>
    public bool HasErrors => ErrorMessages.Count > 0;

    /// <summary>
    /// Gets the error messages from the stored procedure execution.
    /// </summary>
    public IReadOnlyList<string> ErrorMessages { get; }

    /// <summary>
    /// Gets a value indicating whether the stored procedure execution was successful.
    /// </summary>
    public bool IsSuccess => !HasErrors;

    /// <summary>
    /// Gets the first data table from the result data set, or null if no data was returned.
    /// </summary>
    public DataTable? FirstTable => DataSet?.Tables.Count > 0 ? DataSet.Tables[0] : null;

    /// <summary>
    /// Gets the first row from the first data table, or null if no data was returned.
    /// </summary>
    public DataRow? FirstRow => FirstTable?.Rows.Count > 0 ? FirstTable.Rows[0] : null;

    /// <summary>
    /// Gets a specific output parameter value by name.
    /// </summary>
    /// <typeparam name="T">The expected type of the parameter value.</typeparam>
    /// <param name="parameterName">The name of the output parameter.</param>
    /// <returns>The parameter value cast to the specified type, or default if not found or null.</returns>
    public T? GetOutputParameter<T>(string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        
        if (OutputParameters.TryGetValue(parameterName, out object? value))
        {
            if (value is T typedValue)
                return typedValue;
            
            if (value is not null)
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (InvalidCastException)
                {
                    // Return default value if conversion fails
                }
            }
        }
        
        return default;
    }

    /// <summary>
    /// Gets the return value cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The expected type of the return value.</typeparam>
    /// <returns>The return value cast to the specified type, or default if null or casting fails.</returns>
    public T? GetReturnValue<T>()
    {
        if (ReturnValue is T typedValue)
            return typedValue;
        
        if (ReturnValue is not null)
        {
            try
            {
                return (T)Convert.ChangeType(ReturnValue, typeof(T));
            }
            catch (InvalidCastException)
            {
                // Return default value if conversion fails
            }
        }
        
        return default;
    }

    /// <summary>
    /// Creates a successful result instance.
    /// </summary>
    /// <param name="outputParameters">The output parameter values.</param>
    /// <param name="returnValue">The return value.</param>
    /// <param name="dataSet">The result data set.</param>
    /// <param name="rowsAffected">The number of rows affected.</param>
    /// <param name="executionTime">The execution time.</param>
    /// <returns>A successful stored procedure result.</returns>
    internal static StoredProcedureResult Success(
        IDictionary<string, object?> outputParameters,
        object? returnValue,
        DataSet? dataSet,
        int rowsAffected,
        TimeSpan executionTime)
    {
        return new StoredProcedureResult(outputParameters, returnValue, dataSet, rowsAffected, executionTime);
    }

    /// <summary>
    /// Creates a failed result instance.
    /// </summary>
    /// <param name="errorMessages">The error messages.</param>
    /// <param name="executionTime">The execution time.</param>
    /// <returns>A failed stored procedure result.</returns>
    internal static StoredProcedureResult Failure(IList<string> errorMessages, TimeSpan executionTime)
    {
        return new StoredProcedureResult(
            new Dictionary<string, object?>(),
            null,
            null,
            0,
            executionTime,
            errorMessages);
    }
}