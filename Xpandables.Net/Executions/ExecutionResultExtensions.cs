﻿/*******************************************************************************
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
using System.Reflection;

using Microsoft.Extensions.Hosting;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Executions;

/// <summary>
/// Provides extension methods for operation results.
/// </summary>
public static partial class ExecutionResultExtensions
{
    private const int _minSuccessStatusCode = 200;
    private const int _maxSuccessStatusCode = 299;

    /// <summary>
    /// Contains the key for the exception in the <see cref="ElementCollection"/>.
    /// </summary>
    public const string ExceptionKey = "Exception";

    private static readonly MethodInfo ToExecutionResultMethod =
        typeof(ExecutionResultExtensions).GetMethod(nameof(ToExecutionResult),
            BindingFlags.Static | BindingFlags.Public,
            [typeof(IExecutionResult)])!;

    /// <summary>  
    /// Converts the specified execution result to an <see cref="IExecutionResult{TResult}"/>.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="executionResult">The execution result to convert.</param>  
    /// <returns>An <see cref="IExecutionResult{TResult}"/> representing the 
    /// execution result.</returns>  
    public static IExecutionResult<TResult> ToExecutionResult<TResult>(
       this IExecutionResult executionResult)
    {
        IExecutionResult<TResult> result = new ExecutionResult<TResult>
        {
            StatusCode = executionResult.StatusCode,
            Result = executionResult.Result is TResult value ? value : default,
            Errors = executionResult.Errors,
            Headers = executionResult.Headers,
            Extensions = executionResult.Extensions,
            Detail = executionResult.Detail,
            Title = executionResult.Title,
            Location = executionResult.Location
        };

        return result;
    }

    /// <summary>
    /// Converts the specified execution result to an 
    /// <see cref="ExecutionResultException"/>.
    /// </summary>
    /// <param name="executionResult">The execution result to convert.</param>
    /// <returns>An <see cref="ExecutionResultException"/> representing the 
    /// execution result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the execution 
    /// result is a success status code.</exception>
    public static ExecutionResultException ToExecutionResultException(
        this IExecutionResult executionResult)
    {
        if (executionResult.IsSuccessStatusCode())
        {
            throw new InvalidOperationException(
                "The execution result is not a failure status code.");
        }

        return new ExecutionResultException(executionResult);
    }

    /// <summary>
    /// Converts the specified validation result to an <see cref="IExecutionResult"/>.
    /// </summary>
    /// <param name="validationResult">The validation result to convert.</param>
    /// <returns>An <see cref="IExecutionResult"/> representing the 
    /// validation result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the 
    /// validation result is not valid.</exception>
    public static IExecutionResult ToExecutionResult(
        this ValidationResult validationResult)
    {
        if (validationResult.ErrorMessage is null
            || !validationResult.MemberNames.Any())
        {
            throw new InvalidOperationException(
                "The validation result is not valid.");
        }

        ElementCollection errors = validationResult.ToElementCollection();

        return ExecutionResults
            .BadRequest()
            .WithTitle(HttpStatusCode.BadRequest.GetTitle())
            .WithDetail(HttpStatusCode.BadRequest.GetDetail())
            .WithErrors(errors)
            .Build();
    }

    /// <summary>
    /// Converts the specified collection of validation results to an 
    /// <see cref="IExecutionResult"/>.
    /// </summary>
    /// <param name="validationResults">The collection of validation results 
    /// to convert.</param>
    /// <returns>An <see cref="IExecutionResult"/> representing the validation 
    /// results.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the validation 
    /// results are not valid.</exception>
    public static IExecutionResult ToExecutionResult(
        this IEnumerable<ValidationResult> validationResults)
    {
        if (!validationResults.Any())
        {
            throw new InvalidOperationException(
                "The validation results are not valid.");
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
    /// Converts a <see cref="UnauthorizedAccessException"/> to an 
    /// <see cref="IExecutionResult"/>.  
    /// </summary>  
    /// <param name="exception">The exception to convert.</param>  
    /// <returns>An <see cref="IExecutionResult"/> representing the 
    /// execution result.</returns>  
    public static IExecutionResult ToExecutionResult(
        this UnauthorizedAccessException exception)
    {
        bool isDevelopment = (Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT") ?? Environments.Development) ==
            Environments.Development;

        return ExecutionResults
            .Unauthorized()
            .WithTitle(HttpStatusCode.Unauthorized.GetTitle())
            .WithDetail(isDevelopment ? exception.Message : HttpStatusCode.Unauthorized.GetDetail())
            .WithErrors(GetErrors())
#if DEBUG
            .WithException(exception)
#endif
            .Build();

        ElementCollection GetErrors()
        {
            if (exception.InnerException is ValidationException validationException)
            {
                return validationException.ValidationResult.ToElementCollection();
            }

            return ElementCollection.Empty;
        }
    }

    /// <summary>  
    /// Converts a <see cref="InvalidOperationException"/> to an 
    /// <see cref="IExecutionResult"/>.  
    /// </summary>  
    /// <param name="exception">The exception to convert.</param>  
    /// <returns>An <see cref="IExecutionResult"/> representing the 
    /// execution result.</returns>  
    public static IExecutionResult ToExecutionResult(
        this InvalidOperationException exception)
    {
        bool isDevelopment = (Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT") ?? Environments.Development) ==
            Environments.Development;

        return ExecutionResults
            .InternalServerError()
            .WithTitle(isDevelopment ? exception.Message : HttpStatusCode.InternalServerError.GetTitle())
            .WithDetail(isDevelopment ? $"{exception}" : HttpStatusCode.InternalServerError.GetDetail())
            .WithErrors(GetErrors())
#if DEBUG
            .WithException(exception)
#endif
            .Build();

        ElementCollection GetErrors()
        {
            if (exception.InnerException is ValidationException validationException)
            {
                return validationException.ValidationResult.ToElementCollection();
            }

            return ElementCollection.Empty;
        }
    }

    /// <summary>
    /// Converts the specified exception to an <see cref="IExecutionResult"/>.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <remarks>For best practices, this method manages only two types of exceptions :
    /// <see cref="InvalidOperationException"/> and <see cref="ValidationException"/>.</remarks>
    /// <returns>An <see cref="IExecutionResult"/> representing the exception.</returns>
    public static IExecutionResult ToExecutionResult(this Exception exception)
    {
        if (exception is ExecutionResultException executionException)
        {
            return executionException.ExecutionResult;
        }

        return exception switch
        {
            InvalidOperationException invalidOperation =>
                invalidOperation.ToExecutionResult(),
            ValidationException validation => validation
                .ValidationResult.ToExecutionResult(),
            UnauthorizedAccessException accessException =>
                accessException.ToExecutionResult(),
            _ => new InvalidOperationException(exception.Message, exception)
                .ToExecutionResult()
            // this statement must be unreachable, otherwise, it is a bug.
        };
    }

    /// <summary>
    /// Converts the current instance to a generic one with the specified type.
    /// </summary>
    /// <param name="executionResult">The current instance.</param>
    /// <param name="genericType">The underlying type.</param>
    /// <returns>A new instance of <see cref="IExecutionResult{TResult}"/>
    /// .</returns>
    public static dynamic ToExecutionResult(
        this IExecutionResult executionResult,
        Type genericType)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        ArgumentNullException.ThrowIfNull(genericType);

        return ToExecutionResultMethod
            .MakeGenericMethod(genericType)
            .Invoke(null, [executionResult])!;
    }

    /// <summary>
    /// Converts the specified validation result to an 
    /// <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="validationResult">The validation result to convert.</param>
    /// <returns>An <see cref="ElementCollection"/> representing the 
    /// validation result.</returns>
    public static ElementCollection ToElementCollection(
        this ValidationResult validationResult)
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
    /// Converts the specified collection of validation results to an 
    /// <see cref="ElementCollection"/>.
    /// </summary>
    /// <param name="validationResults">The collection of validation results 
    /// to convert.</param>
    /// <returns>An <see cref="ElementCollection"/> representing the 
    /// validation results.</returns>
    public static ElementCollection ToElementCollection(
        this IEnumerable<ValidationResult> validationResults)
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
    /// Converts the specified execution result to a dictionary of element 
    /// extensions.
    /// </summary>
    /// <param name="executionResult">The execution result to convert.</param>
    /// <returns>A dictionary of element extensions representing the execution 
    /// result.</returns>
    public static IDictionary<string, object?> ToElementExtensions(
        this IExecutionResult executionResult)
    {
        ArgumentNullException.ThrowIfNull(executionResult);

        if (!executionResult.Extensions.Any())
        {
            return new Dictionary<string, object?>();
        }

        return executionResult
            .Extensions
            .ToDictionary(
            entry => entry.Key,
            entry => (object?)string.Join(" ", entry.Values));
    }

    /// <summary>
    /// Gets the title of the operation result based on its status code.
    /// </summary>
    /// <param name="statusCode">The status code to get the title for.</param>
    /// <returns>The title of the operation result.</returns>
    public static string GetTitle(this HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.OK => "Success",
            HttpStatusCode.Created => "Created",
            HttpStatusCode.Accepted => "Accepted",
            HttpStatusCode.NoContent => "No Content",
            HttpStatusCode.MovedPermanently => "Moved Permanently",
            HttpStatusCode.Found => "Found",
            HttpStatusCode.SeeOther => "See Other",
            HttpStatusCode.NotModified => "Not Modified",
            HttpStatusCode.TemporaryRedirect => "Temporary Redirect",
            HttpStatusCode.PermanentRedirect => "Permanent Redirect",
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
            HttpStatusCode.NotAcceptable => "Not Acceptable",
            HttpStatusCode.ProxyAuthenticationRequired => "Proxy Authentication Required",
            HttpStatusCode.RequestTimeout => "Request Timeout",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.Gone => "Gone",
            HttpStatusCode.LengthRequired => "Length Required",
            HttpStatusCode.PreconditionFailed => "Precondition Failed",
            HttpStatusCode.RequestEntityTooLarge => "Request Entity Too Large",
            HttpStatusCode.RequestUriTooLong => "Request-URI Too Long",
            HttpStatusCode.UnsupportedMediaType => "Unsupported Media Type",
            HttpStatusCode.RequestedRangeNotSatisfiable => "Requested Range Not Satisfiable",
            HttpStatusCode.ExpectationFailed => "Expectation Failed",
            HttpStatusCode.UpgradeRequired => "Upgrade Required",
            HttpStatusCode.InternalServerError => "Internal Server Error",
            HttpStatusCode.NotImplemented => "Not Implemented",
            HttpStatusCode.BadGateway => "Bad Gateway",
            HttpStatusCode.ServiceUnavailable => "Service Unavailable",
            HttpStatusCode.GatewayTimeout => "Gateway Timeout",
            HttpStatusCode.HttpVersionNotSupported => "HTTP Version Not Supported",
            HttpStatusCode.VariantAlsoNegotiates => "Variant Also Negotiates",
            HttpStatusCode.InsufficientStorage => "Insufficient Storage",
            HttpStatusCode.LoopDetected => "Loop Detected",
            HttpStatusCode.NotExtended => "Not Extended",
            HttpStatusCode.NetworkAuthenticationRequired => "Network Authentication Required",
            HttpStatusCode.PartialContent => "Partial Content",
            HttpStatusCode.MultipleChoices => "Multiple Choices",
            HttpStatusCode.UnprocessableEntity => "Unprocessable Entity",
            HttpStatusCode.Locked => "Locked",
            HttpStatusCode.FailedDependency => "Failed Dependency",
            HttpStatusCode.PreconditionRequired => "Precondition Required",
            HttpStatusCode.TooManyRequests => "Too Many Requests",
            HttpStatusCode.RequestHeaderFieldsTooLarge => "Request Header Fields Too Large",
            HttpStatusCode.UnavailableForLegalReasons => "Unavailable For Legal Reasons",
            HttpStatusCode.Continue => "Continue",
            HttpStatusCode.SwitchingProtocols => "Switching Protocols",
            HttpStatusCode.Processing => "Processing",
            HttpStatusCode.EarlyHints => "Early Hints",
            HttpStatusCode.IMUsed => "IM Used",
            HttpStatusCode.NonAuthoritativeInformation => "Non-Authoritative Information",
            HttpStatusCode.ResetContent => "Reset Content",
            HttpStatusCode.AlreadyReported => "Already Reported",
            HttpStatusCode.MisdirectedRequest => "Misdirected Request",
            HttpStatusCode.Unused => "Unused",
            HttpStatusCode.MultiStatus => "Multi-Status",
            HttpStatusCode.UseProxy => "Use Proxy",
            HttpStatusCode.PaymentRequired => "Payment Required",
            _ => "Unknown"
        };

#pragma warning disable IDE0072 // Add missing cases
    /// <summary>
    /// Gets the detail of the operation result based on its status code.
    /// </summary>
    /// <param name="statusCode">The status code to get the detail for.</param>
    /// <returns>The detail of the operation result.</returns>
    public static string GetDetail(this HttpStatusCode statusCode) =>
            statusCode switch
            {
                HttpStatusCode.InternalServerError or HttpStatusCode.Unauthorized
                => "Please refer to the errors/or contact administrator for additional details",
                _ => "Please refer to the errors property for additional details",
            };
#pragma warning restore IDE0072 // Add missing cases

    /// <summary>    
    /// Determines whether the specified HTTP status code is a success status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a success status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode) =>
        (int)statusCode is >= _minSuccessStatusCode and <= _maxSuccessStatusCode;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a success status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a success status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsSuccessStatusCode(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a failure status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a failure status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsFailureStatusCode(this HttpStatusCode statusCode) =>
        !statusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a failure status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a failure status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsFailureStatusCode(this IExecutionResult executionResult) =>
        !executionResult.StatusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a Created status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a Created status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsCreated(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.Created;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a Created status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a Created status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsCreated(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsCreated();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a Not Found 
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a Not Found 
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNotFound(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.NotFound;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a Not Found status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a Not Found status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNotFound(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsNotFound();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a No Content 
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a No Content 
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNoContent(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.NoContent;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a No Content status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a No Content status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNoContent(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsNoContent();

    /// <summary>    
    /// Determines whether the result of the specified execution result is a file.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the result of the execution result is a file;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsResultFile(this IExecutionResult executionResult) =>
       executionResult.Result is ResultFile;

    /// <summary>    
    /// Determines whether the specified HTTP status code is a Bad Request 
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a Bad Request
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsBadRequest(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.BadRequest;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a Bad Request status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a Bad Request status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsBadRequest(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsBadRequest();

    /// <summary>    
    /// Determines whether the specified HTTP status code is an Unauthorized   
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is an Unauthorized   
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsUnauthorized(this HttpStatusCode statusCode) =>
       statusCode == HttpStatusCode.Unauthorized;

    /// <summary>  
    /// Determines whether the status code of the specified execution result  
    /// is an Unauthorized status code.  
    /// </summary>  
    /// <param name="executionResult">The execution result to check.</param>  
    /// <returns><see langword="true"/> if the status code of the execution  
    /// result is an Unauthorized status code;  
    /// otherwise, <see langword="false"/>.</returns>  
    public static bool IsUnauthorized(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsUnauthorized();

    /// <summary>    
    /// Determines whether the specified HTTP status code is an Internal 
    /// Server Error status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is an Internal 
    /// Server Error status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsInternalServerError(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.InternalServerError;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is an Internal Server Error status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is an Internal Server Error status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsInternalServerError(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsInternalServerError();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a Service Unavailable 
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a Service Unavailable 
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsServiceUnavailable(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.ServiceUnavailable;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a Service Unavailable status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a Service Unavailable status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsServiceUnavailable(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsServiceUnavailable();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a validation problem
    /// status code.
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a validation problem
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsValidationProblemRequest(this HttpStatusCode statusCode) =>
        (int)statusCode is >= (int)HttpStatusCode.BadRequest and <= (int)HttpStatusCode.InternalServerError;

    /// <summary>
    /// Converts an <see cref="Action"/> to an <see cref="IExecutionResult"/>.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>An <see cref="IExecutionResult"/> representing the result of 
    /// the action.</returns>
    public static IExecutionResult ToExecutionResult(this Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action();
            return ExecutionResults.Ok().Build();
        }
        catch (Exception exception)
            when (exception is not ExecutionResultException)
        {
            return exception.ToExecutionResult();
        }
    }

    /// <summary>
    /// Converts an <see cref="Action{T}"/> to an <see cref="IExecutionResult"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument passed to the action.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="args">The argument to pass to the action.</param>
    /// <returns>An <see cref="IExecutionResult"/> representing the result 
    /// of the action.</returns>
    public static IExecutionResult ToExecutionResult<T>(
        this Action<T> action,
        T args)
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
    /// Converts a <see cref="Task"/> to an <see cref="IExecutionResult"/>
    /// asynchronously.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <returns>A <see cref="Task{IOperationResult}"/> representing the result
    /// of the task.</returns>
    public static async Task<IExecutionResult> ToExecutionResultAsync(this Task task)
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
    /// <see cref="IExecutionResult{TResult}"/>
    /// asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <returns>A <see cref="Task{T}"/> representing the result of the task.</returns>
    public static async Task<IExecutionResult<TResult>> ToExecutionResultAsync<TResult>(
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
            return exception
                .ToExecutionResult()
                .ToExecutionResult<TResult>();
        }
    }

    /// <summary>
    /// Converts a <see cref="Func{TResult}"/> to an 
    /// <see cref="IExecutionResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the 
    /// function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>An <see cref="IExecutionResult{TResult}"/> representing the 
    /// result of the function.</returns>
    public static IExecutionResult<TResult> ToExecutionResult<TResult>(
        this Func<TResult> func)
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
            return exception
                .ToExecutionResult()
                .ToExecutionResult<TResult>();
        }
    }

}
