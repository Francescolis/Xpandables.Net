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
/// Configuration options for <see cref="DataLoggingCommandInterceptor"/>.
/// </summary>
/// <remarks>
/// <para>
/// These options control the logging behavior of the default command interceptor.
/// Register via <c>AddXDataCommandInterceptor(options => { ... })</c> or
/// <c>services.Configure&lt;DataCommandInterceptorOptions&gt;(options => { ... })</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// services.AddXDataCommandInterceptor(options =>
/// {
///     options.EnableSensitiveDataLogging = true;
///     options.CategoryName = "MyApp.Database";
///     options.SlowCommandThreshold = TimeSpan.FromSeconds(2);
/// });
/// </code>
/// </example>
public sealed class DataCommandInterceptorOptions
{
	/// <summary>
	/// Gets or sets whether to include parameter values in log output.
	/// </summary>
	/// <remarks>
	/// When <see langword="false"/> (default), only parameter names are logged (e.g., <c>@p0, @p1</c>).
	/// When <see langword="true"/>, parameter names and values are logged (e.g., <c>@p0='Widget', @p1=9.99</c>).
	/// <para>
	/// <strong>Warning:</strong> Enabling this may expose sensitive data (passwords, tokens, PII)
	/// in log output. Use only in development or controlled environments.
	/// </para>
	/// </remarks>
	public bool EnableSensitiveDataLogging { get; set; }

	/// <summary>
	/// Gets or sets a custom log category name.
	/// </summary>
	/// <remarks>
	/// When <see langword="null"/> (default), the full type name of
	/// <see cref="DataLoggingCommandInterceptor"/> is used as the log category.
	/// Set this to route database logs to a specific category for filtering or separate sinks.
	/// </remarks>
	public string? CategoryName { get; set; }

	/// <summary>
	/// Gets or sets the duration threshold beyond which a command is logged at
	/// <see cref="Microsoft.Extensions.Logging.LogLevel.Warning"/> instead of
	/// <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/>.
	/// </summary>
	/// <remarks>
	/// When <see langword="null"/> (default), all successful commands are logged at
	/// <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/>.
	/// When set, commands exceeding this threshold are logged at
	/// <see cref="Microsoft.Extensions.Logging.LogLevel.Warning"/> to help identify slow queries.
	/// </remarks>
	public TimeSpan? SlowCommandThreshold { get; set; }
}
