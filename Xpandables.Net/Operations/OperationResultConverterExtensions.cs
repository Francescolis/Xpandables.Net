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

using Xpandables.Net.Collections;

namespace Xpandables.Net.Operations;
/// <summary>
/// Provides extension methods for converting operation results to exceptions.
/// </summary>
public static partial class OperationResultExtensions
{
    /// <summary>  
    /// Converts the specified operation result to an <see cref="IOperationResult{TResult}"/>.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="operationResult">The operation result to convert.</param>  
    /// <returns>An <see cref="IOperationResult{TResult}"/> representing the 
    /// operation result.</returns>  
    public static IOperationResult<TResult> ToOperationResult<TResult>(
       this IOperationResult operationResult) =>
       new OperationResult<TResult>
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

        ElementCollection errors = ElementCollection.With(validationResult
            .MemberNames
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => new ElementEntry(s, validationResult.ErrorMessage))
            .ToList());

        return OperationResults
            .BadRequest()
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

        ElementCollection errors = ElementCollection.With(validationResults
            .Where(s => s.ErrorMessage is not null && s.MemberNames.Any())
            .SelectMany(s => s.MemberNames
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => new ElementEntry(m, s.ErrorMessage ?? string.Empty)))
            .ToList());

        return OperationResults
            .BadRequest()
            .WithErrors(errors)
            .Build();
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

        IFailureBuilder builder = exception switch
        {
            InvalidOperationException => OperationResults.InternalServerError(),
            ValidationException => OperationResults.BadRequest(),
            _ => OperationResults.Failure(System.Net.HttpStatusCode.Unused)
            // this statement must be unreachable, otherwise, it is a bug.
        };

        return builder
            .WithDetail(exception.Message)
            .WithException(exception)
            .Build();
    }
}