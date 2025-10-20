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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security;
using System.Security.Authentication;
using System.Text.Json;

using Xpandables.Net;
using Xpandables.Net.Collections;

namespace Xpandables.Net.ExecutionResults;

/// <summary>
/// Provides extension methods for mapping exceptions to HTTP status codes.
/// </summary>
/// <remarks>These methods enable consistent translation of common .NET exceptions to standard HTTP status codes,
/// which can be useful when building web APIs or services that need to communicate error conditions to clients. The
/// mappings are based on typical usage patterns and may be customized as needed for specific application
/// requirements.</remarks>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class ExceptionExtensions
{
    /// <summary>
    /// Extension methods for mapping exceptions to HTTP status codes.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    extension(Exception exception)
    {
        /// <summary>
        /// Creates an <see cref="ExecutionResult"/> representing the outcome of the current exception, optionally using
        /// a specified HTTP status code and reason phrase.
        /// </summary>
        /// <remarks>In development environments, the execution result includes detailed exception
        /// information for easier debugging. In other environments, only generic error details are provided to avoid
        /// exposing sensitive information.</remarks>
        /// <param name="statusCode">The HTTP status code to associate with the execution result. If <see langword="null"/>, the status code is
        /// determined from the exception.</param>
        /// <param name="reason">An optional reason phrase to include in the execution result. If <see langword="null"/>, the exception
        /// message or status code title is used depending on the environment.</param>
        /// <returns>An <see cref="ExecutionResult"/> describing the failure, including status code, error details, and exception
        /// information.</returns>
        public ExecutionResult ToExecutionResult(HttpStatusCode? statusCode = null, string? reason = default)
        {
            ArgumentNullException.ThrowIfNull(exception);

            bool isDevelopment = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development";

            if (exception is ExecutionResultException executionResultException)
            {
                var executionResult = executionResultException.ExecutionResult;
                return ExecutionResult
                    .Failure(executionResult.StatusCode)
                    .WithTitle(executionResult.StatusCode.Title)
                    .WithDetail(executionResult.StatusCode.Detail)
                    .WithErrors(executionResult.Errors)
                    .WithExtensions(executionResult.Extensions)
                    .WithHeaders(executionResult.Headers)
                    .Build();
            }

            statusCode ??= exception.GetHttpStatusCode();

            return ExecutionResult
                .Failure(statusCode.Value)
                .WithTitle(isDevelopment ? reason ?? exception.Message : statusCode.Value.Title)
                .WithDetail(isDevelopment ? $"{exception}" : statusCode.Value.Detail)
                .WithErrors(GetElementEntries(exception))
                .WithException(exception)
                .Build();

        }

        /// <summary>
        /// Retrieves a collection of element entries extracted from the associated exception and its inner exceptions,
        /// including validation and JSON-based error details.
        /// </summary>
        /// <remarks>This method traverses the exception tree, including aggregate and inner exceptions,
        /// to collect validation errors and errors represented in JSON format. The returned collection may include
        /// entries from multiple sources within the exception hierarchy. </remarks>
        /// <returns>An <see cref="ElementCollection"/> containing all element entries found within the exception hierarchy.
        /// Returns <see cref="ElementCollection.Empty"/> if no entries are found.</returns>
        public ElementCollection GetElementEntries()
        {
            ArgumentNullException.ThrowIfNull(exception);

            // Collect entries first, then merge once
            var buffer = new List<ElementEntry>(capacity: 8);

            var stack = new Stack<Exception>();
            var visited = new HashSet<Exception>();

            // Seed traversal with AggregateException flattened if needed
            if (exception is AggregateException agg)
            {
                foreach (var ex in agg.Flatten().InnerExceptions)
                {
                    if (ex is not null) stack.Push(ex);
                }
            }
            else
            {
                stack.Push(exception);
            }

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is null || !visited.Add(current))
                {
                    continue;
                }

                if (current is ValidationException validationException)
                {
                    var coll = validationException.ValidationResult.ToElementCollection();
                    if (!coll.IsEmpty)
                    {
                        foreach (var entry in coll)
                        {
                            buffer.Add(entry);
                        }
                    }
                }
                else
                {
                    TryCollectJsonErrors(current.Message, buffer);
                }

                // Traverse children
                if (current is AggregateException currentAgg)
                {
                    foreach (var inner in currentAgg.Flatten().InnerExceptions)
                    {
                        if (inner is not null) stack.Push(inner);
                    }
                }
                else if (current.InnerException is not null)
                {
                    stack.Push(current.InnerException);
                }
            }

            return buffer.Count == 0 ? ElementCollection.Empty : ElementCollection.With(buffer);

            // Local helper: uses AnonymousFromJsonString to extract Dictionary<string, IEnumerable<string>>
            static void TryCollectJsonErrors(string? message, List<ElementEntry> sink)
            {
                if (string.IsNullOrWhiteSpace(message))
                    return;

                // Quick pre-check to avoid parsing obvious non-JSON messages
                var trimmed = message.AsSpan().Trim();
                if (trimmed.Length == 0)
                    return;

                char first = trimmed[0];
                if (first != '{' && first != '[')
                    return;

#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    using var doc = JsonDocument.Parse(message);
                    var root = doc.RootElement;

                    if (root.ValueKind != JsonValueKind.Object)
                        return;

                    if (!TryGetPropertyIgnoreCase(root, "errors", out var errorsProp))
                        return;

                    if (errorsProp.ValueKind != JsonValueKind.Object)
                        return;

                    foreach (var property in errorsProp.EnumerateObject())
                    {
                        var key = property.Name;
                        if (string.IsNullOrWhiteSpace(key))
                            continue;

                        var value = property.Value;
                        if (value.ValueKind == JsonValueKind.String)
                        {
                            var s = value.GetString();
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                sink.Add(new ElementEntry(key, s!));
                            }
                        }
                        else if (value.ValueKind == JsonValueKind.Array)
                        {
                            List<string>? list = null;
                            foreach (var item in value.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String)
                                {
                                    var s = item.GetString();
                                    if (!string.IsNullOrWhiteSpace(s))
                                    {
                                        list ??= [];
                                        list.Add(s!);
                                    }
                                }
                            }

                            if (list is { Count: > 0 })
                            {
                                sink.Add(new ElementEntry(key, [.. list]));
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore JSON parse errors and move on
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            static bool TryGetPropertyIgnoreCase(JsonElement obj, string name, out JsonElement value)
            {
                if (obj.TryGetProperty(name, out value))
                    return true;

                foreach (var prop in obj.EnumerateObject())
                {
                    if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = prop.Value;
                        return true;
                    }
                }

                value = default;
                return false;
            }
        }

        /// <summary>
        /// Maps the provided exception to an appropriate HTTP status code.
        /// </summary>
        /// <returns>The corresponding HTTP status code.</returns>
        public HttpStatusCode GetHttpStatusCode()
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception switch
            {
                // 400 - Bad Request (Client errors)
                ArgumentNullException => HttpStatusCode.BadRequest,
                ArgumentOutOfRangeException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                ValidationException => HttpStatusCode.BadRequest,
                FormatException => HttpStatusCode.BadRequest,

                // 401 - Unauthorized
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                AuthenticationException => HttpStatusCode.Unauthorized,

                // 403 - Forbidden
                SecurityException => HttpStatusCode.Forbidden,

                // 404 - Not Found
                FileNotFoundException => HttpStatusCode.NotFound,
                DirectoryNotFoundException => HttpStatusCode.NotFound,
                KeyNotFoundException => HttpStatusCode.NotFound,

                // 405 - Method Not Allowed
                NotSupportedException => HttpStatusCode.MethodNotAllowed,

                // 408 - Request Timeout
                TimeoutException => HttpStatusCode.RequestTimeout,

                // 409 - Conflict
                IOException => HttpStatusCode.Conflict,
                DuplicateNameException => HttpStatusCode.Conflict,

                // 410 - Gone
                // No direct .NET exception maps well to Gone

                // 412 - Precondition Failed
                VersionNotFoundException => HttpStatusCode.PreconditionFailed,

                // 413 - Request Entity Too Large
                // No direct .NET exception maps well

                // 415 - Unsupported Media Type
                // No direct .NET exception maps well

                // 422 - Unprocessable Entity
                InvalidDataException => (HttpStatusCode)422, // Unprocessable Entity

                // 423 - Locked
                SynchronizationLockException => (HttpStatusCode)423, // Locked

                // 428 - Precondition Required
                // No direct .NET exception maps well

                // 429 - Too Many Requests
                // No direct .NET exception maps well

                // 502 - Bad Gateway
                WebException => HttpStatusCode.BadGateway,

                // 500 - Internal Server Error (Server errors - default case)
                NullReferenceException => HttpStatusCode.InternalServerError,
                StackOverflowException => HttpStatusCode.InternalServerError,
                OutOfMemoryException => HttpStatusCode.InternalServerError,
                InvalidOperationException => HttpStatusCode.InternalServerError,
                ApplicationException => HttpStatusCode.InternalServerError,

                // 501 - Not Implemented
                NotImplementedException => HttpStatusCode.NotImplemented,

                // 503 - Service Unavailable
                InvalidProgramException => HttpStatusCode.ServiceUnavailable,

                // 504 - Gateway Timeout
                TaskCanceledException => HttpStatusCode.GatewayTimeout,
                OperationCanceledException => HttpStatusCode.GatewayTimeout,

                // Default case
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}
