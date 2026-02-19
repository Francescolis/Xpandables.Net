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
/// Provides a base no-op implementation of <see cref="IDataCommandInterceptor"/>.
/// </summary>
/// <remarks>
/// <para>
/// All methods are virtual and return <see cref="ValueTask.CompletedTask"/> by default,
/// allowing derived classes to override only the methods they need.
/// </para>
/// <para>
/// The default DI registration uses <see cref="DataLoggingCommandInterceptor"/> (which derives
/// from this class) to provide structured logging out of the box.
/// Use <c>AddXDataCommandInterceptor&lt;T&gt;()</c> to replace it with a custom implementation,
/// or use <see cref="Default"/> for a no-op interceptor when constructing repositories directly.
/// </para>
/// </remarks>
public class DataCommandInterceptor : IDataCommandInterceptor
{
	/// <summary>
	/// Gets a shared default no-op interceptor instance.
	/// </summary>
	public static DataCommandInterceptor Default { get; } = new();

	/// <inheritdoc />
	public virtual ValueTask CommandExecutingAsync(
		DataCommandContext context,
		CancellationToken cancellationToken = default) =>
		ValueTask.CompletedTask;

	/// <inheritdoc />
	public virtual ValueTask CommandExecutedAsync(
		DataCommandContext context,
		TimeSpan duration,
		int? rowsAffected,
		CancellationToken cancellationToken = default) =>
		ValueTask.CompletedTask;

	/// <inheritdoc />
	public virtual ValueTask CommandFailedAsync(
		DataCommandContext context,
		TimeSpan duration,
		Exception exception,
		CancellationToken cancellationToken = default) =>
		ValueTask.CompletedTask;
}
