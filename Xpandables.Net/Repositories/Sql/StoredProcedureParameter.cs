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

namespace Xpandables.Net.Repositories.Sql;

/// <summary>
/// Represents a parameter for a stored procedure with comprehensive metadata support.
/// </summary>
internal sealed class StoredProcedureParameter
{
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the value of the parameter.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Gets the SQL database type of the parameter.
    /// </summary>
    public SqlDbType DbType { get; init; }

    /// <summary>
    /// Gets the direction of the parameter (Input, Output, InputOutput, ReturnValue).
    /// </summary>
    public ParameterDirection Direction { get; init; } = ParameterDirection.Input;

    /// <summary>
    /// Gets the maximum size of the parameter data.
    /// </summary>
    public int? Size { get; init; }

    /// <summary>
    /// Gets the precision for decimal and numeric parameters.
    /// </summary>
    public byte? Precision { get; init; }

    /// <summary>
    /// Gets the scale for decimal and numeric parameters.
    /// </summary>
    public byte? Scale { get; init; }

    /// <summary>
    /// Gets the user-defined table type name for table-valued parameters.
    /// </summary>
    public string? TypeName { get; init; }

    /// <summary>
    /// Gets a value indicating whether this parameter is a table-valued parameter.
    /// </summary>
    public bool IsTableValued => !string.IsNullOrEmpty(TypeName);

    /// <summary>
    /// Creates a new instance of <see cref="StoredProcedureParameter"/> for input parameters.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="dbType">The SQL database type.</param>
    /// <param name="size">The parameter size.</param>
    /// <returns>A new parameter instance.</returns>
    public static StoredProcedureParameter CreateInput(string name, object? value, SqlDbType? dbType = null, int? size = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        return new StoredProcedureParameter
        {
            Name = name,
            Value = value,
            DbType = dbType ?? InferDbType(value),
            Direction = ParameterDirection.Input,
            Size = size
        };
    }

    /// <summary>
    /// Creates a new instance of <see cref="StoredProcedureParameter"/> for output parameters.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="dbType">The SQL database type.</param>
    /// <param name="size">The parameter size.</param>
    /// <param name="precision">The parameter precision.</param>
    /// <param name="scale">The parameter scale.</param>
    /// <returns>A new parameter instance.</returns>
    public static StoredProcedureParameter CreateOutput(string name, SqlDbType dbType, int? size = null, byte? precision = null, byte? scale = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        return new StoredProcedureParameter
        {
            Name = name,
            Value = null,
            DbType = dbType,
            Direction = ParameterDirection.Output,
            Size = size,
            Precision = precision,
            Scale = scale
        };
    }

    /// <summary>
    /// Creates a new instance of <see cref="StoredProcedureParameter"/> for input/output parameters.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The initial parameter value.</param>
    /// <param name="dbType">The SQL database type.</param>
    /// <param name="size">The parameter size.</param>
    /// <param name="precision">The parameter precision.</param>
    /// <param name="scale">The parameter scale.</param>
    /// <returns>A new parameter instance.</returns>
    public static StoredProcedureParameter CreateInputOutput(string name, object? value, SqlDbType dbType, int? size = null, byte? precision = null, byte? scale = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        return new StoredProcedureParameter
        {
            Name = name,
            Value = value,
            DbType = dbType,
            Direction = ParameterDirection.InputOutput,
            Size = size,
            Precision = precision,
            Scale = scale
        };
    }

    /// <summary>
    /// Creates a new instance of <see cref="StoredProcedureParameter"/> for return value parameters.
    /// </summary>
    /// <returns>A new return value parameter instance.</returns>
    public static StoredProcedureParameter CreateReturnValue()
    {
        return new StoredProcedureParameter
        {
            Name = "@ReturnValue",
            Value = null,
            DbType = SqlDbType.Int,
            Direction = ParameterDirection.ReturnValue
        };
    }

    /// <summary>
    /// Creates a new instance of <see cref="StoredProcedureParameter"/> for table-valued parameters.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The table data.</param>
    /// <param name="typeName">The user-defined table type name.</param>
    /// <returns>A new table-valued parameter instance.</returns>
    public static StoredProcedureParameter CreateTableValued(string name, object? value, string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        
        return new StoredProcedureParameter
        {
            Name = name,
            Value = value,
            DbType = SqlDbType.Structured,
            Direction = ParameterDirection.Input,
            TypeName = typeName
        };
    }

    /// <summary>
    /// Infers the SQL database type from the provided value.
    /// </summary>
    /// <param name="value">The value to infer the type from.</param>
    /// <returns>The inferred SQL database type.</returns>
    private static SqlDbType InferDbType(object? value)
    {
        return value switch
        {
            null => SqlDbType.NVarChar,
            string => SqlDbType.NVarChar,
            int => SqlDbType.Int,
            long => SqlDbType.BigInt,
            short => SqlDbType.SmallInt,
            byte => SqlDbType.TinyInt,
            bool => SqlDbType.Bit,
            DateTime => SqlDbType.DateTime2,
            DateTimeOffset => SqlDbType.DateTimeOffset,
            DateOnly => SqlDbType.Date,
            TimeOnly => SqlDbType.Time,
            decimal => SqlDbType.Decimal,
            double => SqlDbType.Float,
            float => SqlDbType.Real,
            Guid => SqlDbType.UniqueIdentifier,
            byte[] => SqlDbType.VarBinary,
            char => SqlDbType.NChar,
            TimeSpan => SqlDbType.Time,
            _ => SqlDbType.NVarChar
        };
    }
}