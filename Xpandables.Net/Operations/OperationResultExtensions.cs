
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.I18n;

namespace Xpandables.Net.Operations;

/// <summary>
/// Provides a set of static methods for <see cref="IOperationResult"/>.
/// </summary>
public static class OperationResultExtensions
{
    private const int _minSuccessStatusCode = 200;
    private const int _maxSuccessStatusCode = 299;
    /// <summary>
    /// Defines the key for the exception in the <see cref="ElementCollection"/>.
    /// </summary>
    public const string ExceptionKey = "Exception";

    /// <summary>
    /// Determines whether the specified status code is a success one.
    /// </summary>
    /// <param name="statusCode">The status code to act on.</param>
    /// <returns><see langword="true"/> if the status is success, 
    /// otherwise returns <see langword="false"/></returns>
    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        => (int)statusCode is >= _minSuccessStatusCode
            and <= _maxSuccessStatusCode;

    /// <summary>
    /// Determines whether the specified status code is a failure one.
    /// </summary>
    /// <param name="statusCode">The status code to act on.</param>
    /// <returns><see langword="true"/> if the status is failure, 
    /// otherwise returns <see langword="false"/></returns>
    public static bool IsFailureStatusCode(this HttpStatusCode statusCode)
        => !IsSuccessStatusCode(statusCode);

    /// <summary>
    /// Ensures that the specified status code is a success code.
    /// Throws an exception if the status code is not a success.
    /// </summary>
    /// <param name="statusCode">The status code value to be checked.</param>
    /// <returns>Returns the status code if it's a success code 
    /// or throws an <see cref="InvalidOperationException"/> exception.</returns>
    /// <exception cref="InvalidOperationException">The code 
    /// <paramref name="statusCode"/> is not a success status code.</exception>
    public static HttpStatusCode EnsureSuccessStatusCode(
        this HttpStatusCode statusCode)
    {
        if (!IsSuccessStatusCode(statusCode))
            throw new InvalidOperationException(
                $"The code '{statusCode}' is not a success status code.",
                new ArgumentOutOfRangeException(
                    nameof(statusCode),
                    $"{statusCode}",
                    $"The status code must be greater or " +
                    $"equal to {_minSuccessStatusCode} and " +
                    $"lower or equal to {_maxSuccessStatusCode}"));

        return statusCode;
    }

    /// <summary>
    /// Ensures that the specified status code is a failure code.
    /// Throws an exception if the status code is not a failure; 
    /// </summary>
    /// <param name="statusCode">The status code value to be checked.</param>
    /// <returns>Returns the status code if it's a failure code 
    /// or throws an <see cref="InvalidOperationException"/> exception.</returns>
    /// <exception cref="InvalidOperationException">The code 
    /// <paramref name="statusCode"/> is not a failure status code.</exception>
    public static HttpStatusCode EnsureFailureStatusCode(
        this HttpStatusCode statusCode)
    {
        if (!IsFailureStatusCode(statusCode))
            throw new InvalidOperationException(
                $"The code '{statusCode}' is not a failure status code",
                new ArgumentOutOfRangeException(
                    nameof(statusCode),
                    $"{statusCode}",
                    $"The status code must be greater " +
                    $"than {_maxSuccessStatusCode} or " +
                    $"lower than {_minSuccessStatusCode}"));

        return statusCode;
    }

    /// <summary>
    /// Converts the current <see cref="IOperationResult"/> 
    /// to <see cref="OperationResultException"/>.
    /// </summary>
    /// <param name="operationResult">The operation result to be 
    /// converted.</param>
    /// <returns>An instance of <see cref="OperationResultException"/>
    /// with the result.</returns>
    public static OperationResultException ToOperationResultException(
        this IOperationResult operationResult)
    {
        ArgumentNullException.ThrowIfNull(operationResult);

        return new OperationResultException(operationResult);
    }

    /// <summary>
    /// Converts the current <see cref="ValidationResult"/> 
    /// to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="instance">The validation result to act on.</param>
    /// <returns>An implementation of <see cref="IOperationResult"/> with 
    /// <see cref="IOperationResult.StatusCode"/> = 
    /// <see cref="HttpStatusCode.BadRequest"/>
    /// if <see cref="ValidationResult.ErrorMessage"/> and 
    /// <see cref="ValidationResult.MemberNames"/> are not null, 
    /// otherwise throws an <see cref="InvalidOperationException"/>.</returns>
    public static IOperationResult ToOperationResult(
        this ValidationResult instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (instance.ErrorMessage is null ||
            !instance.MemberNames.Any())
            throw new InvalidOperationException(
                "ErrorMessage or MemberNames is null !");

        ElementCollection errors = [];
        foreach (string memberName in instance.MemberNames)
            if (!string.IsNullOrEmpty(memberName))
                errors.Add(memberName, instance.ErrorMessage);

        return OperationResults
            .BadRequest()
            .WithErrors(errors)
            .Build();
    }

    /// <summary>
    /// Converts the current <see cref="ValidationException"/> 
    /// to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="instance">The validation exception to act on.</param>
    /// <returns>An implementation of <see cref="IOperationResult"/> 
    /// with <see cref="IOperationResult.StatusCode"/> = 
    /// <see cref="HttpStatusCode.BadRequest"/>
    /// if <see cref="ValidationResult.ErrorMessage"/> and 
    /// <see cref="ValidationResult.MemberNames"/> are not null, 
    /// otherwise throws an <see cref="InvalidOperationException"/>.</returns>
    public static IOperationResult ToOperationResult(
        this ValidationException instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        return instance.ValidationResult.ToOperationResult();
    }

    /// <summary>
    /// Converts the current <see cref="Exception"/> 
    /// to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="exception">The validation exception to act on.</param>
    /// <returns>An implementation of <see cref="IOperationResult"/> with 
    /// <see cref="IOperationResult.StatusCode"/> = 
    /// <see cref="HttpStatusCode.InternalServerError"/> 
    /// or <see cref="HttpStatusCode.BadRequest"/>.</returns>
    public static IOperationResult ToOperationResult(
        this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        IOperationResult.IFailureBuilder builder
            = exception is InvalidOperationException
            ? OperationResults.InternalError()
            : OperationResults.BadRequest();

        return builder
            .WithDetail(exception.Message)
            .WithException(exception)
            .Build();
    }

    /// <summary>
    /// Converts the current <see cref="ValueTask"/> 
    /// to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="valueTask">The task to act on.</param>
    /// <returns>An instance of <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="valueTask"/> is null.</exception>
    public static async ValueTask<IOperationResult> ToOperationResultAsync(
        this ValueTask valueTask)
    {
        ArgumentNullException.ThrowIfNull(valueTask);

        try
        {
            await valueTask.ConfigureAwait(false);
            return OperationResults.Ok().Build();
        }
        catch (OperationResultException operationResultException)
        {
            return operationResultException.Operation;
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception.ToOperationResult();
        }
    }

    /// <summary>
    /// Converts the current <see cref="Task"/> 
    /// to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="task">The task to act on.</param>
    /// <returns>An instance of <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="task"/> is null.</exception>
    public static async ValueTask<IOperationResult> ToOperationResultAsync(
        this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            await task.ConfigureAwait(false);
            return OperationResults.Ok().Build();
        }
        catch (OperationResultException operationResultException)
        {
            return operationResultException.Operation;
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception.ToOperationResult();
        }
    }

    /// <summary>
    /// Converts the current <see cref="ValueTask{TResult}"/> 
    /// to a <see cref="IOperationResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="valueTask">The task to act on.</param>
    /// <returns>An instance of <see cref="IOperationResult{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="valueTask"/> is null.</exception>
    public static async ValueTask<IOperationResult<TResult>>
        ToOperationResultAsync<TResult>(
        this ValueTask<TResult> valueTask)
    {
        ArgumentNullException.ThrowIfNull(valueTask);

        try
        {
            TResult result = await valueTask.ConfigureAwait(false);
            return result is { }
                ? OperationResults
                    .Ok(result)
                    .Build()
                : OperationResults
                    .BadRequest<TResult>()
                    .WithError(
                        typeof(TResult).Name,
                        I18nXpandables.OperationResultValueIsNull)
                    .Build();
        }
        catch (OperationResultException operationResultException)
        {
            return operationResultException.Operation
                .ToOperationResult<TResult>();
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception
                .ToOperationResult()
                .ToOperationResult<TResult>();
        }
    }

    /// <summary>
    /// Converts the current <see cref="Task{TResult}"/> 
    /// to a <see cref="IOperationResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="task">The task to act on.</param>
    /// <returns>An instance of <see cref="IOperationResult{TResult}"/>
    /// .</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="task"/> 
    /// is null.</exception>
    public static async ValueTask<IOperationResult<TResult>>
        ToOperationResultAsync<TResult>(
        this Task<TResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            TResult result = await task.ConfigureAwait(false);
            return result is { }
                ? OperationResults
                    .Ok(result)
                    .Build()
                : OperationResults
                    .BadRequest<TResult>()
                    .WithError(
                        typeof(TResult).Name,
                        I18nXpandables.OperationResultValueIsNull)
                    .Build();
        }
        catch (OperationResultException operationResultException)
        {
            return operationResultException.Operation
                .ToOperationResult<TResult>();
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception
                .ToOperationResult()
                .ToOperationResult<TResult>();
        }
    }
}
