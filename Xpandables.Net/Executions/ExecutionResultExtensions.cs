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
/// Provides extension methods for operation results.
/// </summary>
public static partial class ExecutionResultExtensions
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
            .WithTitle(HttpStatusCode.BadRequest.GetAppropriateTitle())
            .WithDetail(HttpStatusCode.BadRequest.GetAppropriateDetail())
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
            .WithTitle(isDevelopment ? reason ?? exception.Message : statusCode.Value.GetAppropriateTitle())
            .WithDetail(isDevelopment ? $"{exception}" : statusCode.Value.GetAppropriateDetail())
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
    public static ExecutionResultException ToExecutionResultException(this ExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);

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

    /// <summary>
    /// Converts an <see cref="Action"/> to an <see cref="ExecutionResult"/>.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>An <see cref="ExecutionResult"/> representing the result of 
    /// the action.</returns>
    public static ExecutionResult ToExecutionResult(this Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action();
            return ExecutionResults.Success();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }

    /// <summary>
    /// Converts an <see cref="Action{T}"/> to an <see cref="ExecutionResult"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument passed to the action.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="args">The argument to pass to the action.</param>
    /// <returns>An <see cref="ExecutionResult"/> representing the result 
    /// of the action.</returns>
    public static ExecutionResult ToExecutionResult<T>(this Action<T> action, T args)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action(args);
            return ExecutionResults.Ok().Build();
        }
        catch (ValidationException validationException)
        {
            return validationException.ToExecutionResult();
        }
        catch (ExecutionResultException executionException)
        {
            return executionException.ExecutionResult;
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }

    /// <summary>
    /// Converts a <see cref="Task"/> to an <see cref="ExecutionResult"/>
    /// asynchronously.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <returns>A <see cref="Task{IOperationResult}"/> representing the result
    /// of the task.</returns>
    public static async Task<ExecutionResult> ToExecutionResultAsync(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            await task.ConfigureAwait(false);
            return ExecutionResults.Ok().Build();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }

    /// <summary>
    /// Converts a <see cref="Task{TResult}"/> to an 
    /// <see cref="ExecutionResult{TResult}"/>
    /// asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <returns>A <see cref="Task{T}"/> representing the result of the task.</returns>
    public static async Task<ExecutionResult<TResult>> ToExecutionResultAsync<TResult>(
        this Task<TResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            TResult result = await task.ConfigureAwait(false);
            return ExecutionResults
                .Ok(result)
                .Build();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }

    /// <summary>
    /// Converts a <see cref="Func{TResult}"/> to an 
    /// <see cref="ExecutionResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the 
    /// function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>An <see cref="ExecutionResult{TResult}"/> representing the 
    /// result of the function.</returns>
    public static ExecutionResult<TResult> ToExecutionResult<TResult>(this Func<TResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            TResult result = func();
            return ExecutionResults
                .Ok(result)
                .Build();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
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
