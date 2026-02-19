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
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace System.Data;

/// <summary>
/// Provides a default logging implementation of <see cref="IDataCommandInterceptor"/>
/// using structured <see cref="ILogger"/> output with <c>[LoggerMessage]</c> source generation.
/// </summary>
/// <remarks>
/// <para>
/// This interceptor is registered automatically by <c>AddXDataCommandInterceptor()</c>.
/// It logs command execution at the following levels:
/// <list type="bullet">
/// <item><see cref="LogLevel.Debug"/> — before execution (SQL text and parameter names).</item>
/// <item><see cref="LogLevel.Information"/> — after successful execution (duration and row count).</item>
/// <item><see cref="LogLevel.Warning"/> — after successful execution that exceeds
/// <see cref="DataCommandInterceptorOptions.SlowCommandThreshold"/>.</item>
/// <item><see cref="LogLevel.Error"/> — when execution fails (duration and exception).</item>
/// </list>
/// </para>
/// <para>
/// Configure behavior via <see cref="DataCommandInterceptorOptions"/>:
/// <list type="bullet">
/// <item><see cref="DataCommandInterceptorOptions.EnableSensitiveDataLogging"/> — include parameter values in logs.</item>
/// <item><see cref="DataCommandInterceptorOptions.CategoryName"/> — custom log category.</item>
/// <item><see cref="DataCommandInterceptorOptions.SlowCommandThreshold"/> — slow command warning threshold.</item>
/// </list>
/// </para>
/// </remarks>
public sealed partial class DataLoggingCommandInterceptor : DataCommandInterceptor
{
	private static readonly string DefaultCategoryName = typeof(DataLoggingCommandInterceptor).FullName!;

	private readonly ILogger _logger;
	private readonly DataCommandInterceptorOptions _options;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataLoggingCommandInterceptor"/> class.
	/// </summary>
	/// <param name="loggerFactory">The logger factory used to create a logger with the configured category name.</param>
	/// <param name="options">The interceptor options.</param>
	public DataLoggingCommandInterceptor(
		ILoggerFactory loggerFactory,
		IOptions<DataCommandInterceptorOptions> options)
	{
		ArgumentNullException.ThrowIfNull(loggerFactory);
		ArgumentNullException.ThrowIfNull(options);

		_options = options.Value;
		_logger = loggerFactory.CreateLogger(_options.CategoryName ?? DefaultCategoryName);
	}

	/// <inheritdoc />
	public override ValueTask CommandExecutingAsync(
		DataCommandContext context,
		CancellationToken cancellationToken = default)
	{
		if (_logger.IsEnabled(LogLevel.Debug))
		{
#pragma warning disable CA1873 // FormatParameters is guarded by IsEnabled check above
			LogCommandExecuting(
				_logger,
				context.OperationType,
				context.EntityTypeName,
				FormatParameters(context.Parameters),
				context.CommandText);
#pragma warning restore CA1873
		}

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public override ValueTask CommandExecutedAsync(
		DataCommandContext context,
		TimeSpan duration,
		int? rowsAffected,
		CancellationToken cancellationToken = default)
	{
		if (_options.SlowCommandThreshold.HasValue &&
			duration > _options.SlowCommandThreshold.Value)
		{
			LogCommandExecutedSlow(
				_logger,
				context.OperationType,
				context.EntityTypeName,
				duration.TotalMilliseconds,
				rowsAffected,
				_options.SlowCommandThreshold.Value.TotalMilliseconds,
				context.CommandText);
		}
		else
		{
			LogCommandExecuted(
				_logger,
				context.OperationType,
				context.EntityTypeName,
				duration.TotalMilliseconds,
				rowsAffected,
				context.CommandText);
		}

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public override ValueTask CommandFailedAsync(
		DataCommandContext context,
		TimeSpan duration,
		Exception exception,
		CancellationToken cancellationToken = default)
	{
		LogCommandFailed(
			_logger,
			context.OperationType,
			context.EntityTypeName,
			duration.TotalMilliseconds,
			context.CommandText,
			exception);

		return ValueTask.CompletedTask;
	}

	private string FormatParameters(IReadOnlyList<SqlParameter> parameters)
	{
		if (parameters.Count == 0)
			return string.Empty;

		var sb = new StringBuilder();

		for (var i = 0; i < parameters.Count; i++)
		{
			if (i > 0)
				sb.Append(", ");

			var param = parameters[i];

			if (_options.EnableSensitiveDataLogging)
			{
				sb.Append(param.Name)
					.Append('=')
					.Append(param.Value is null or DBNull ? "NULL" : $"'{param.Value}'");
			}
			else
			{
				sb.Append(param.Name);
			}
		}

		return sb.ToString();
	}

	[LoggerMessage(
		EventId = 1,
		Level = LogLevel.Debug,
		Message = "Executing {OperationType} [{EntityTypeName}] — Parameters: [{Parameters}]\n{CommandText}")]
	private static partial void LogCommandExecuting(
		ILogger logger,
		DataCommandOperationType operationType,
		string? entityTypeName,
		string parameters,
		string commandText);

	[LoggerMessage(
		EventId = 2,
		Level = LogLevel.Information,
		Message = "Executed {OperationType} [{EntityTypeName}] in {DurationMs:F1}ms — {RowsAffected} row(s) affected\n{CommandText}")]
	private static partial void LogCommandExecuted(
		ILogger logger,
		DataCommandOperationType operationType,
		string? entityTypeName,
		double durationMs,
		int? rowsAffected,
		string commandText);

	[LoggerMessage(
		EventId = 3,
		Level = LogLevel.Warning,
		Message = "Slow command {OperationType} [{EntityTypeName}] in {DurationMs:F1}ms (threshold: {ThresholdMs:F0}ms) — {RowsAffected} row(s) affected\n{CommandText}")]
	private static partial void LogCommandExecutedSlow(
		ILogger logger,
		DataCommandOperationType operationType,
		string? entityTypeName,
		double durationMs,
		int? rowsAffected,
		double thresholdMs,
		string commandText);

	[LoggerMessage(
		EventId = 4,
		Level = LogLevel.Error,
		Message = "Failed {OperationType} [{EntityTypeName}] after {DurationMs:F1}ms\n{CommandText}")]
	private static partial void LogCommandFailed(
		ILogger logger,
		DataCommandOperationType operationType,
		string? entityTypeName,
		double durationMs,
		string commandText,
		Exception exception);
}
