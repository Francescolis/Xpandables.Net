
/************************************************************************************************************
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
************************************************************************************************************/
using System.ComponentModel.DataAnnotations;
using System.Net;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Operations;

/// <summary>
/// Provides a set of static methods for <see cref="OperationResultException"/>.
/// </summary>
public static partial class OperationResultExtensions
{
    /// <summary>
    /// Converts the current <see cref="IOperationResult"/> to <see cref="OperationResultException"/>.
    /// </summary>
    /// <param name="operationResult">The operation result to be converted.</param>
    /// <returns>An instance of <see cref="OperationResultException"/> with the result.</returns>
    public static OperationResultException ToOperationResultException(this IOperationResult operationResult)
    {
        ArgumentNullException.ThrowIfNull(operationResult);

        return new OperationResultException(operationResult);
    }

    /// <summary>
    /// Converts the current <see cref="ValidationResult"/> to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="this">The validation result to act on.</param>
    /// <returns>An implementation of <see cref="IOperationResult"/> with 
    /// <see cref="IOperationResult.StatusCode"/> = <see cref="HttpStatusCode.BadRequest"/>
    /// if <see cref="ValidationResult.ErrorMessage"/> and 
    /// <see cref="ValidationResult.MemberNames"/> are not null, 
    /// otherwise throws an <see cref="InvalidOperationException"/>.</returns>
    public static IOperationResult ToOperationResult(this ValidationResult @this)
    {
        ArgumentNullException.ThrowIfNull(@this);

        if (@this.ErrorMessage is null || !@this.MemberNames.Any())
            throw new InvalidOperationException("ErrorMessage or MemberNames is null !");

        ElementCollection errors = [];
        foreach (string memberName in @this.MemberNames)
            if (!string.IsNullOrEmpty(memberName))
                errors.Add(memberName, [@this.ErrorMessage]);

        return OperationResults
            .BadRequest()
            .WithErrors(errors)
            .Build();
    }

    /// <summary>
    /// Converts the current <see cref="ValidationException"/> to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="this">The validation exception to act on.</param>
    /// <returns>An implementation of <see cref="IOperationResult"/> 
    /// with <see cref="IOperationResult.StatusCode"/> = <see cref="HttpStatusCode.BadRequest"/>
    /// if <see cref="ValidationResult.ErrorMessage"/> and 
    /// <see cref="ValidationResult.MemberNames"/> are not null, 
    /// otherwise throws an <see cref="InvalidOperationException"/>.</returns>
    public static IOperationResult ToOperationResult(this ValidationException @this)
    {
        ArgumentNullException.ThrowIfNull(@this);

        return @this.ValidationResult.ToOperationResult();
    }

    /// <summary>
    /// Converts the current <see cref="Exception"/> to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="exception">The validation exception to act on.</param>
    /// <returns>An implementation of <see cref="IOperationResult"/> with 
    /// <see cref="IOperationResult.StatusCode"/> = <see cref="HttpStatusCode.InternalServerError"/> 
    /// or <see cref="HttpStatusCode.BadRequest"/>.</returns>
    public static IOperationResult ToOperationResult(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        IOperationResult.IFailureBuilder builder = exception is InvalidOperationException
            ? OperationResults.InternalError()
            : OperationResults.BadRequest();

        return builder
            .WithDetail(exception.Message)
            .WithError(ElementEntry.UndefinedKey, exception.ToString())
            .Build();
    }

    /// <summary>
    /// Converts the current <see cref="ValueTask"/> to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="valueTask">The task to act on.</param>
    /// <returns>An instance of <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="valueTask"/> is null.</exception>
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
            return operationResultException.OperationResult;
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception.ToOperationResult();
        }
    }

    /// <summary>
    /// Converts the current <see cref="Task"/> to a <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="task">The task to act on.</param>
    /// <returns>An instance of <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="task"/> is null.</exception>
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
            return operationResultException.OperationResult;
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception.ToOperationResult();
        }
    }

    /// <summary>
    /// Converts the current <see cref="ValueTask{TResult}"/> to a <see cref="IOperationResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="valueTask">The task to act on.</param>
    /// <returns>An instance of <see cref="IOperationResult{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="valueTask"/> is null.</exception>
    public static async ValueTask<IOperationResult<TResult>> ToOperationResultAsync<TResult>(
        this ValueTask<TResult> valueTask)
    {
        ArgumentNullException.ThrowIfNull(valueTask);

        try
        {
            TResult result = await valueTask.ConfigureAwait(false);
            return OperationResults.Ok(result).Build();
        }
        catch (OperationResultException operationResultException)
        {
            return operationResultException.OperationResult.ToOperationResult<TResult>();
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception.ToOperationResult().ToOperationResult<TResult>();
        }
    }

    /// <summary>
    /// Converts the current <see cref="Task{TResult}"/> to a <see cref="IOperationResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="task">The task to act on.</param>
    /// <returns>An instance of <see cref="IOperationResult{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="task"/> is null.</exception>
    public static async ValueTask<IOperationResult<TResult>> ToOperationResultAsync<TResult>(
        this Task<TResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            TResult result = await task.ConfigureAwait(false);
            return OperationResults.Ok(result).Build();
        }
        catch (OperationResultException operationResultException)
        {
            return operationResultException.OperationResult.ToOperationResult<TResult>();
        }
        catch (Exception exception)
            when (exception is not OperationResultException)
        {
            return exception.ToOperationResult().ToOperationResult<TResult>();
        }
    }
}
