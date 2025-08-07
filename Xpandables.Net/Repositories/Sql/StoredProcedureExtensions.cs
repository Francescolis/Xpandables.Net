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
/// Provides extension methods for enhanced stored procedure functionality.
/// </summary>
public static class StoredProcedureExtensions
{
    /// <summary>
    /// Adds an input parameter with backward compatibility for the existing WithParameter method.
    /// This method ensures backward compatibility with existing code while providing enhanced functionality.
    /// </summary>
    /// <param name="builder">The stored procedure builder.</param>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static StoredProcedureBuilder WithParameter(this StoredProcedureBuilder builder, string name, object? value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithInputParameter(name, value);
    }

    /// <summary>
    /// Adds multiple input parameters from a dictionary.
    /// </summary>
    /// <param name="builder">The stored procedure builder.</param>
    /// <param name="parameters">A dictionary containing parameter names and values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static StoredProcedureBuilder WithParameters(this StoredProcedureBuilder builder, IDictionary<string, object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(parameters);

        foreach (KeyValuePair<string, object?> kvp in parameters)
        {
            builder.WithInputParameter(kvp.Key, kvp.Value);
        }

        return builder;
    }

    /// <summary>
    /// Configures the stored procedure builder for common output parameters.
    /// </summary>
    /// <param name="builder">The stored procedure builder.</param>
    /// <param name="includeRecordCount">Whether to include a record count output parameter.</param>
    /// <param name="includeErrorMessage">Whether to include an error message output parameter.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static StoredProcedureBuilder WithCommonOutputs(this StoredProcedureBuilder builder, bool includeRecordCount = true, bool includeErrorMessage = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (includeRecordCount)
        {
            builder.WithOutputParameter("RecordCount", SqlDbType.Int);
        }

        if (includeErrorMessage)
        {
            builder.WithOutputParameter("ErrorMessage", SqlDbType.NVarChar, size: 4000);
        }

        return builder;
    }

    /// <summary>
    /// Configures the stored procedure builder with standard pagination parameters.
    /// </summary>
    /// <param name="builder">The stored procedure builder.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static StoredProcedureBuilder WithPagination(this StoredProcedureBuilder builder, int pageIndex, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        return builder
            .WithInputParameter("PageIndex", pageIndex, SqlDbType.Int)
            .WithInputParameter("PageSize", pageSize, SqlDbType.Int);
    }

    /// <summary>
    /// Adds search parameters for text-based filtering.
    /// </summary>
    /// <param name="builder">The stored procedure builder.</param>
    /// <param name="searchTerm">The search term to filter by.</param>
    /// <param name="searchFields">Optional specific fields to search within.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static StoredProcedureBuilder WithSearch(this StoredProcedureBuilder builder, string? searchTerm, params string[] searchFields)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithInputParameter("SearchTerm", searchTerm, SqlDbType.NVarChar, size: 1000);

        if (searchFields.Length > 0)
        {
            builder.WithInputParameter("SearchFields", string.Join(",", searchFields), SqlDbType.NVarChar, size: 4000);
        }

        return builder;
    }

    /// <summary>
    /// Adds date range parameters for filtering by date ranges.
    /// </summary>
    /// <param name="builder">The stored procedure builder.</param>
    /// <param name="startDate">The start date of the range (optional).</param>
    /// <param name="endDate">The end date of the range (optional).</param>
    /// <param name="dateFieldPrefix">The prefix for the date field parameters (default is "Date").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static StoredProcedureBuilder WithDateRange(this StoredProcedureBuilder builder, DateTime? startDate = null, DateTime? endDate = null, string dateFieldPrefix = "Date")
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(dateFieldPrefix);

        if (startDate.HasValue)
        {
            builder.WithInputParameter($"{dateFieldPrefix}From", startDate.Value, SqlDbType.DateTime2);
        }

        if (endDate.HasValue)
        {
            builder.WithInputParameter($"{dateFieldPrefix}To", endDate.Value, SqlDbType.DateTime2);
        }

        return builder;
    }

    /// <summary>
    /// Configures timeout and retry policy with common defaults.
    /// </summary>
    /// <param name="builder">The stored procedure builder.</param>
    /// <param name="timeoutMinutes">The timeout in minutes (default is 5).</param>
    /// <param name="maxRetries">The maximum number of retries (default is 3).</param>
    /// <param name="retryDelaySeconds">The delay between retries in seconds (default is 2).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static StoredProcedureBuilder WithStandardOptions(this StoredProcedureBuilder builder, int timeoutMinutes = 5, int maxRetries = 3, int retryDelaySeconds = 2)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutMinutes, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);
        ArgumentOutOfRangeException.ThrowIfNegative(retryDelaySeconds);

        return builder
            .WithTimeout(TimeSpan.FromMinutes(timeoutMinutes))
            .WithRetryPolicy(maxRetries, TimeSpan.FromSeconds(retryDelaySeconds));
    }
}