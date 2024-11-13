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
using System.Reflection;

using Microsoft.Extensions.Hosting;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Operations;
/// <summary>
/// Provides extension methods for converting operation results to exceptions.
/// </summary>
public static partial class OperationResultExtensions
{
    private static readonly MethodInfo ToOperationResultMethod =
        typeof(OperationResultExtensions).GetMethod(nameof(ToOperationResult),
            BindingFlags.Static | BindingFlags.Public,
            [typeof(IOperationResult)])!;

    /// <summary>  
    /// Converts the specified operation result to an <see cref="IOperationResult{TResult}"/>.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="operationResult">The operation result to convert.</param>  
    /// <returns>An <see cref="IOperationResult{TResult}"/> representing the 
    /// operation result.</returns>  
    public static IOperationResult<TResult> ToOperationResult<TResult>(
       this IOperationResult operationResult)
    {
        IOperationResult<TResult> result = new OperationResult<TResult>
        {
            StatusCode = operationResult.StatusCode,
            Result = (TResult?)operationResult.Result,
            Errors = operationResult.Errors,
            Headers = operationResult.Headers,
            Extensions = operationResult.Extensions,
            Detail = operationResult.Detail,
            Title = operationResult.Title,
            Location = operationResult.Location
        };

        return result;
    }

    /// <summary>
    /// Converts the specified operation result to an 
    /// <see cref="OperationResultException"/>.
    /// </summary>
    /// <param name="operationResult">The operation result to convert.</param>
    /// <returns>An <see cref="OperationResultException"/> representing the 
    /// operation result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the operation 
    /// result is a success status code.</exception>
    public static OperationResultException ToOperationResultException(
        this IOperationResult operationResult)
    {
        if (operationResult.IsSuccessStatusCode())
        {
            throw new InvalidOperationException(
                "The operation result is not a failure status code.");
        }

        return new OperationResultException(operationResult);
    }

    /// <summary>
    /// Converts the specified validation result to an <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="validationResult">The validation result to convert.</param>
    /// <returns>An <see cref="IOperationResult"/> representing the 
    /// validation result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the 
    /// validation result is not valid.</exception>
    public static IOperationResult ToOperationResult(
        this ValidationResult validationResult)
    {
        if (validationResult.ErrorMessage is null
            || !validationResult.MemberNames.Any())
        {
            throw new InvalidOperationException(
                "The validation result is not valid.");
        }

        ElementCollection errors = validationResult.ToElementCollection();

        return OperationResults
            .BadRequest()
            .WithTitle(HttpStatusCode.BadRequest.GetTitle())
            .WithDetail(HttpStatusCode.BadRequest.GetDetail())
            .WithErrors(errors)
            .Build();
    }

    /// <summary>
    /// Converts the specified collection of validation results to an 
    /// <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="validationResults">The collection of validation results 
    /// to convert.</param>
    /// <returns>An <see cref="IOperationResult"/> representing the validation 
    /// results.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the validation 
    /// results are not valid.</exception>
    public static IOperationResult ToOperationResult(
        this IEnumerable<ValidationResult> validationResults)
    {
        if (!validationResults.Any())
        {
            throw new InvalidOperationException(
                "The validation results are not valid.");
        }

        ElementCollection errors = validationResults.ToElementCollection();

        return OperationResults
            .BadRequest()
            .WithTitle(HttpStatusCode.BadRequest.GetTitle())
            .WithDetail(HttpStatusCode.BadRequest.GetDetail())
            .WithErrors(errors)
            .Build();
    }

    /// <summary>  
    /// Converts a <see cref="UnauthorizedAccessException"/> to an 
    /// <see cref="IOperationResult"/>.  
    /// </summary>  
    /// <param name="exception">The exception to convert.</param>  
    /// <returns>An <see cref="IOperationResult"/> representing the 
    /// operation result.</returns>  
    public static IOperationResult ToOperationResult(
        this UnauthorizedAccessException exception)
    {
        bool isDevelopment = (Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT") ?? Environments.Development) ==
            Environments.Development;

        return OperationResults
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
    /// <see cref="IOperationResult"/>.  
    /// </summary>  
    /// <param name="exception">The exception to convert.</param>  
    /// <returns>An <see cref="IOperationResult"/> representing the 
    /// operation result.</returns>  
    public static IOperationResult ToOperationResult(
        this InvalidOperationException exception)
    {
        bool isDevelopment = (Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT") ?? Environments.Development) ==
            Environments.Development;

        return OperationResults
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
    /// Converts the specified exception to an <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <remarks>For best practices, this method manages only two types of exceptions :
    /// <see cref="InvalidOperationException"/> and <see cref="ValidationException"/>.</remarks>
    /// <returns>An <see cref="IOperationResult"/> representing the exception.</returns>
    public static IOperationResult ToOperationResult(this Exception exception)
    {
        if (exception is OperationResultException operationResultException)
        {
            return operationResultException.OperationResult;
        }

        return exception switch
        {
            InvalidOperationException invalidOperation =>
                invalidOperation.ToOperationResult(),
            ValidationException validation => validation
                .ValidationResult.ToOperationResult(),
            UnauthorizedAccessException accessException =>
                accessException.ToOperationResult(),
            _ => new InvalidOperationException(exception.Message, exception)
                .ToOperationResult()
            // this statement must be unreachable, otherwise, it is a bug.
        };
    }

    /// <summary>
    /// Converts the current instance to a generic one with the specified type.
    /// </summary>
    /// <param name="operationResult">The current instance.</param>
    /// <param name="genericType">The underlying type.</param>
    /// <returns>A new instance of <see cref="IOperationResult{TResult}"/>
    /// .</returns>
    public static dynamic ToOperationResult(
        this IOperationResult operationResult,
        Type genericType)
    {
        ArgumentNullException.ThrowIfNull(operationResult);
        ArgumentNullException.ThrowIfNull(genericType);

        return ToOperationResultMethod
            .MakeGenericMethod(genericType)
            .Invoke(null, [operationResult])!;
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

        return ElementCollection.With(validationResult
            .MemberNames
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => new ElementEntry(s, validationResult.ErrorMessage))
            .ToList());
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
        List<ValidationResult> validations = validationResults.ToList();
        if (validations.Count == 0)
        {
            return ElementCollection.Empty;
        }

        return ElementCollection.With(validations
            .Where(s => s.ErrorMessage is not null && s.MemberNames.Any())
            .SelectMany(s => s.MemberNames
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => new ElementEntry(m, s.ErrorMessage ?? string.Empty))
            )
            .ToList());
    }

    /// <summary>
    /// Converts the specified operation result to a dictionary of element 
    /// extensions.
    /// </summary>
    /// <param name="operationResult">The operation result to convert.</param>
    /// <returns>A dictionary of element extensions representing the operation 
    /// result.</returns>
    public static IDictionary<string, object?> ToElementExtensions(
        this IOperationResult operationResult)
    {
        ArgumentNullException.ThrowIfNull(operationResult);

        if (!operationResult.Extensions.Any())
        {
            return new Dictionary<string, object?>();
        }

        return operationResult
            .Extensions
            .ToDictionary(
            entry => entry.Key,
            entry => (object?)string.Join(" ", entry.Values));
    }
}