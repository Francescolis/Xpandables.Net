/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
/// Specifies the type of ADO.NET command execution.
/// </summary>
public enum DataCommandOperationType
{
	/// <summary>
	/// A query that returns rows via <see cref="System.Data.Common.DbCommand.ExecuteReaderAsync(Threading.CancellationToken)"/>.
	/// </summary>
	Reader,

	/// <summary>
	/// A query that returns a single value via <see cref="System.Data.Common.DbCommand.ExecuteScalarAsync(Threading.CancellationToken)"/>.
	/// </summary>
	Scalar,

	/// <summary>
	/// A command that returns the number of affected rows via <see cref="System.Data.Common.DbCommand.ExecuteNonQueryAsync(Threading.CancellationToken)"/>.
	/// </summary>
	NonQuery
}

/// <summary>
/// Represents the context of a database command for interceptor callbacks.
/// </summary>
/// <param name="CommandText">The SQL command text being executed.</param>
/// <param name="Parameters">The parameters associated with the command.</param>
/// <param name="OperationType">The type of ADO.NET execution being performed.</param>
/// <param name="EntityTypeName">The name of the entity type the repository operates on, if available.</param>
public readonly record struct DataCommandContext(
	string CommandText,
	IReadOnlyList<SqlParameter> Parameters,
	DataCommandOperationType OperationType,
	string? EntityTypeName = null);

/// <summary>
/// Defines a contract for intercepting database command execution in <see cref="DataRepository{TData}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Implementations can observe command execution for logging, telemetry, diagnostics, or auditing.
/// All methods are invoked around ADO.NET command execution:
/// <list type="bullet">
/// <item><see cref="CommandExecutingAsync"/> — called before the command is executed.</item>
/// <item><see cref="CommandExecutedAsync"/> — called after successful execution with timing and row count.</item>
/// <item><see cref="CommandFailedAsync"/> — called when execution throws, with timing and exception.</item>
/// </list>
/// </para>
/// <para>
/// A default logging implementation (<see cref="DataLoggingCommandInterceptor"/>) is registered
/// automatically via <c>AddXDataUnitOfWork()</c>. Use <c>AddXDataCommandInterceptor&lt;T&gt;()</c>
/// to replace it with a custom implementation.
/// </para>
/// </remarks>
public interface IDataCommandInterceptor
{
	/// <summary>
	/// Called before a database command is executed.
	/// </summary>
	/// <param name="context">The command context containing SQL text, parameters, and operation type.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	ValueTask CommandExecutingAsync(
		DataCommandContext context,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Called after a database command has been executed successfully.
	/// </summary>
	/// <param name="context">The command context containing SQL text, parameters, and operation type.</param>
	/// <param name="duration">The elapsed time of the command execution.</param>
	/// <param name="rowsAffected">The number of rows affected, or <see langword="null"/> for reader operations.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	ValueTask CommandExecutedAsync(
		DataCommandContext context,
		TimeSpan duration,
		int? rowsAffected,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Called when a database command execution fails with an exception.
	/// </summary>
	/// <param name="context">The command context containing SQL text, parameters, and operation type.</param>
	/// <param name="duration">The elapsed time before the failure occurred.</param>
	/// <param name="exception">The exception that was thrown during execution.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	ValueTask CommandFailedAsync(
		DataCommandContext context,
		TimeSpan duration,
		Exception exception,
		CancellationToken cancellationToken = default);
}
