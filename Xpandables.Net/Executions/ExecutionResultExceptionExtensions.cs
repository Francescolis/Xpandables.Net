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
using System.Net;

using Microsoft.Extensions.Hosting;

using Xpandables.Net.Collections;
using Xpandables.Net.Text;

namespace Xpandables.Net.Executions;

/// <summary>
/// Extension methods to convert exception to <see cref="ExecutionResult"/>.
/// </summary>
public static class ExecutionResultExceptionExtensions
{
    /// <summary>
    /// Converts the specified <see cref="ValidationResult"/> to an <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="validationResult">The validation result to convert.</param>
    /// <returns>An <see cref="ElementCollection"/> representing the 
    /// validation result.</returns>
    public static ElementCollection ToElementCollection(this ValidationResult validationResult)
    {
        if (validationResult.ErrorMessage is null
            || !validationResult.MemberNames.Any())
        {
            return ElementCollection.Empty;
        }

        return ElementCollection.With([.. validationResult
            .MemberNames
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => new ElementEntry(s, validationResult.ErrorMessage))]);
    }

    /// <summary>
    /// Converts the specified collection of <see cref="ValidationResult"/>s to an 
    /// <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="validationResults">The collection of validation results 
    /// to convert.</param>
    /// <returns>An <see cref="ElementCollection"/> representing the 
    /// validation results.</returns>
    public static ElementCollection ToElementCollection(this IEnumerable<ValidationResult> validationResults)
    {
        List<ValidationResult> validations = [.. validationResults];
        if (validations.Count == 0)
        {
            return ElementCollection.Empty;
        }

        return ElementCollection.With([.. validations
            .Where(s => s.ErrorMessage is not null && s.MemberNames.Any())
            .SelectMany(s => s.MemberNames
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => new ElementEntry(m, s.ErrorMessage ?? string.Empty))
            )]);
    }

    /// <summary>
    /// Converts the specified collection of validation results to an <see cref="ExecutionResult"/>.
    /// </summary>
    /// <param name="validationResults">The collection of validation results 
    /// to convert.</param>
    /// <returns>An <see cref="ExecutionResult"/> representing the validation results.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the validation 
    /// results are not valid.</exception>
    public static ExecutionResult ToExecutionResult(this IEnumerable<ValidationResult> validationResults)
    {
        if (!validationResults.Any())
        {
            throw new InvalidOperationException(
                "The validation results is empty.");
        }

        ElementCollection errors = validationResults.ToElementCollection();

        return ExecutionResults
            .BadRequest()
            .WithTitle(HttpStatusCode.BadRequest.GetTitle())
            .WithDetail(HttpStatusCode.BadRequest.GetDetail())
            .WithErrors(errors)
            .Build();
    }

    /// <summary>  
    /// Converts a <see cref="Exception"/> to an <see cref="ExecutionResult"/>.  
    /// </summary>  
    /// <param name="exception">The exception to convert.</param> 
    /// <param name="statusCode">Optional HTTP status code to use.</param>
    /// <param name="reason">Optional reason phrase for the status code.</param>
    /// <returns>An <see cref="ExecutionResult"/> representing the exception.</returns>  
    public static ExecutionResult ToExecutionResult(
        this Exception exception, HttpStatusCode? statusCode = null, string? reason = default)
    {
        bool isDevelopment = (Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT") ?? Environments.Development) ==
            Environments.Development;

        statusCode ??= exception.GetAppropriatStatusCode();

        return ExecutionResults
            .Failure(statusCode.Value)
            .WithTitle(isDevelopment ? reason ?? exception.Message : statusCode.Value.GetTitle())
            .WithDetail(isDevelopment ? $"{exception}" : statusCode.Value.GetDetail())
            .WithErrors(GetValidationExceptionErrors(exception))
            .WithException(exception)
            .Build();
    }

    /// <summary>
    /// Converts the specified <see cref="ExecutionResult"/> to an <see cref="ExecutionResultException"/>.
    /// </summary>
    /// <param name="executionResult">The execution result to convert.</param>
    /// <returns>An <see cref="ExecutionResultException"/> representing the 
    /// execution result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the execution 
    /// result is a success status code.</exception>
    public static ExecutionResultException ToExecutionResultException(this ExecutionResult executionResult)
    {
        if (executionResult.IsSuccessStatusCode())
        {
            throw new InvalidOperationException(
                "The execution result must be a failure result.");
        }

        return new ExecutionResultException(executionResult);
    }

    /// <summary>
    /// Converts the specified <see cref="ElementCollection"/> to a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="element">The element collection to convert.</param>
    /// <returns>A dictionary of items representing the element collection.</returns>
    public static IDictionary<string, object?> ToDictionary(this ElementCollection element)
    {
        if (!element.Any())
        {
            return new Dictionary<string, object?>();
        }

        return element
            .ToDictionary(
            entry => entry.Key,
            entry => (object?)entry.Values.StringJoin(" "));
    }

    private static ElementCollection GetValidationExceptionErrors(Exception exception)
    {
        Stack<Exception> stack = new();
        stack.Push(exception);

        while (stack.Count > 0)
        {
            Exception currentException = stack.Pop();

            if (currentException is ValidationException validationException)
            {
                return validationException.ValidationResult.ToElementCollection();
            }

            var anonymousType = new { Errors = default(Dictionary<string, IEnumerable<string>>) };
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var errors = currentException.Message.DeserializeAnonymousType(anonymousType, DefaultSerializerOptions.Defaults);
                if (errors is not null && errors.Errors is not null && errors.Errors.Count > 0)
                {
                    ElementCollection collection = [];
                    foreach (var error in errors.Errors)
                    {
                        collection.Add(error.Key, [.. error.Value]);
                    }

                    return collection;
                }
            }
            catch
            {
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (currentException.InnerException is not null)
            {
                stack.Push(currentException.InnerException);
            }
        }

        return ElementCollection.Empty;
    }
}
