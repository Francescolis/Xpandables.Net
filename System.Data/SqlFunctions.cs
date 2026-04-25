/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
/// Provides SQL aggregate function markers for use in <see cref="DataSpecification"/> selector expressions.
/// These methods are never executed at runtime; they are translated to SQL aggregate functions
/// by <see cref="DataSqlBuilderBase"/>.
/// </summary>
/// <example>
/// <code>
/// var spec = DataSpecification.For&lt;OrderData&gt;()
///     .GroupBy(o =&gt; o.Status)
///     .Select(o =&gt; new
///     {
///         Status = o.Status,
///         Total = SqlFunctions.Count(),
///         MaxAmount = SqlFunctions.Max(o.Amount)
///     });
/// </code>
/// </example>
public static class SqlFunctions
{
	private const string MarkerMethodError =
		"This method is a SQL translation marker and must only be used inside DataSpecification selector expressions. " +
		"It cannot be invoked directly.";

	/// <summary>
	/// Translates to SQL <c>COUNT(*)</c>.
	/// </summary>
	/// <returns>This method is never executed; it serves as a translation marker.</returns>
	/// <exception cref="InvalidOperationException">Always thrown if called directly.</exception>
	public static int Count() => throw new InvalidOperationException(MarkerMethodError);

	/// <summary>
	/// Translates to SQL <c>COUNT(column)</c>.
	/// </summary>
	/// <typeparam name="T">The type of the column expression.</typeparam>
	/// <param name="column">The column expression to count (non-null values).</param>
	/// <returns>This method is never executed; it serves as a translation marker.</returns>
	/// <exception cref="InvalidOperationException">Always thrown if called directly.</exception>
	public static int Count<T>(T column) => throw new InvalidOperationException(MarkerMethodError);

	/// <summary>
	/// Translates to SQL <c>COUNT(DISTINCT column)</c>.
	/// </summary>
	/// <typeparam name="T">The type of the column expression.</typeparam>
	/// <param name="column">The column expression to count distinct values of.</param>
	/// <returns>This method is never executed; it serves as a translation marker.</returns>
	/// <exception cref="InvalidOperationException">Always thrown if called directly.</exception>
	public static int CountDistinct<T>(T column) => throw new InvalidOperationException(MarkerMethodError);

	/// <summary>
	/// Translates to SQL <c>SUM(column)</c>.
	/// </summary>
	/// <typeparam name="T">The numeric type of the column expression.</typeparam>
	/// <param name="column">The column expression to sum.</param>
	/// <returns>This method is never executed; it serves as a translation marker.</returns>
	/// <exception cref="InvalidOperationException">Always thrown if called directly.</exception>
	public static T Sum<T>(T column) => throw new InvalidOperationException(MarkerMethodError);

	/// <summary>
	/// Translates to SQL <c>AVG(column)</c>.
	/// </summary>
	/// <typeparam name="T">The numeric type of the column expression.</typeparam>
	/// <param name="column">The column expression to average.</param>
	/// <returns>This method is never executed; it serves as a translation marker.</returns>
	/// <exception cref="InvalidOperationException">Always thrown if called directly.</exception>
	public static T Avg<T>(T column) => throw new InvalidOperationException(MarkerMethodError);

	/// <summary>
	/// Translates to SQL <c>MIN(column)</c>.
	/// </summary>
	/// <typeparam name="T">The type of the column expression.</typeparam>
	/// <param name="column">The column expression to find the minimum of.</param>
	/// <returns>This method is never executed; it serves as a translation marker.</returns>
	/// <exception cref="InvalidOperationException">Always thrown if called directly.</exception>
	public static T Min<T>(T column) => throw new InvalidOperationException(MarkerMethodError);

	/// <summary>
	/// Translates to SQL <c>MAX(column)</c>.
	/// </summary>
	/// <typeparam name="T">The type of the column expression.</typeparam>
	/// <param name="column">The column expression to find the maximum of.</param>
	/// <returns>This method is never executed; it serves as a translation marker.</returns>
	/// <exception cref="InvalidOperationException">Always thrown if called directly.</exception>
	public static T Max<T>(T column) => throw new InvalidOperationException(MarkerMethodError);
}
