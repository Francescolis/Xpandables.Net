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
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace System.ExecutionResults;

/// <summary>
/// Provides extension methods for mapping exceptions to HTTP status codes.
/// </summary>
/// <remarks>These methods enable consistent translation of common .NET exceptions to standard HTTP status codes,
/// which can be useful when building web APIs or services that need to communicate error conditions to clients. The
/// mappings are based on typical usage patterns and may be customized as needed for specific application
/// requirements.</remarks>
public static class ExceptionExtensions
{
    /// <summary>
    /// Extension methods for mapping exceptions to HTTP status codes.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    extension(Exception exception)
    {
        /// <summary>
        /// Creates an <see cref="OperationResult"/> representing the outcome of the current exception, optionally using
        /// a specified HTTP status code and reason phrase.
        /// </summary>
        /// <remarks>In development environments, the operation result includes detailed exception
        /// information for easier debugging. In other environments, only generic error details are provided to avoid
        /// exposing sensitive information.</remarks>
        /// <param name="statusCode">The HTTP status code to associate with the operation result. If <see langword="null"/>, the status code is
        /// determined from the exception.</param>
        /// <param name="reason">An optional reason phrase to include in the operation result. If <see langword="null"/>, the exception
        /// message or status code title is used depending on the environment.</param>
        /// <returns>An <see cref="OperationResult"/> describing the failure, including status code, error details, and exception
        /// information.</returns>
        public OperationResult ToOperationResult(HttpStatusCode? statusCode = null, string? reason = default)
        {
            ArgumentNullException.ThrowIfNull(exception);

            bool isDevelopment = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development";

            if (exception is OperationResultException operationException)
            {
                var executionResult = operationException.OperationResult;
                return OperationResult
                    .FailureStatus(executionResult.StatusCode)
                    .WithTitle(executionResult.StatusCode.Title)
                    .WithDetail(executionResult.StatusCode.Detail)
                    .WithErrors(executionResult.Errors)
                    .WithExtensions(executionResult.Extensions)
                    .WithHeaders(executionResult.Headers)
                    .Build();
            }

            statusCode ??= exception.GetHttpStatusCode();

            var builder = OperationResult
                .FailureStatus(statusCode.Value)
                .WithTitle(isDevelopment ? reason ?? exception.Message : statusCode.Value.Title)
                .WithDetail(isDevelopment ? $"{exception}" : statusCode.Value.Detail)
                .WithErrors(exception.GetElementEntries());

            return exception is ValidationException
                ? builder.Build()
                : builder.WithException(exception).Build();
        }
    }
}
