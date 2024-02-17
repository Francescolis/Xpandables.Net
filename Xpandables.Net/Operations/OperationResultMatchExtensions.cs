
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
namespace Xpandables.Net.Operations;

/// <summary>
/// Provides a set of static methods for <see cref="IOperationResultMatch"/> 
/// and <see cref="IOperationResultMatch{TResult}"/> implementations.
/// </summary>
public static partial class OperationResultExtensions
{
    /// <summary>
    /// Returns a new instance of <see cref="IOperationResultMatch"/> that allows 
    /// user to apply pattern matching on the <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="operationResult">The <see cref="IOperationResult"/> instance to act on.</param>
    /// <returns>An instance of <see cref="IOperationResultMatch"/>.</returns>
    public static IOperationResultMatch Match(this IOperationResult operationResult)
        => new OperationResultMatch(operationResult);

    /// <summary>
    /// Returns a new instance of <see cref="IOperationResultMatch{TResult}"/> that allows 
    /// user to apply pattern matching on the <see cref="IOperationResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of thr result.</typeparam>
    /// <param name="operationResult">The <see cref="IOperationResult{TResult}"/> instance to act on.</param>
    /// <returns>An instance of <see cref="IOperationResultMatch{TResult}"/>.</returns>
    public static IOperationResultMatch<TResult> Match<TResult>(this IOperationResult<TResult> operationResult)
        => new OperationResultMatch<TResult>(operationResult);

    /// <summary>
    /// Applies the specified function if the result is a success one.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="operation"/> 
    /// or <paramref name="onSuccess"/> is null.</exception>
    public static IOperationResult Success(
        this IOperationResult operation,
        Func<IOperationResult, IOperationResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return operation
            .Match()
            .Success(onSuccess);
    }

    /// <summary>
    /// Applies the specified action if the result is a success one.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="operation">The operation to act on.</param>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="operation"/> 
    /// or <paramref name="onSuccess"/> is null.</exception>
    public static IOperationResult<TResult> Success<TResult>(
        this IOperationResult<TResult> operation,
        Func<IOperationResult<TResult>, IOperationResult<TResult>> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return operation
            .Match()
            .Success(onSuccess);
    }

    /// <summary>
    /// Applies the specified function if the result is a failure one.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="operation"/> 
    /// or <paramref name="onFailure"/> is null.</exception>
    public static IOperationResult Failure(
        this IOperationResult operation,
        Func<IOperationResult, IOperationResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(onFailure);

        return operation
            .Match()
            .Failure(onFailure);
    }

    /// <summary>
    /// Applies the specified function if the result is a failure one.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="operationResult">The operation to act on.</param>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="operationResult"/> 
    /// or <paramref name="onFailure"/> is null.</exception>
    public static IOperationResult<TResult> Failure<TResult>(
        this IOperationResult<TResult> operationResult,
        Func<IOperationResult<TResult>, IOperationResult<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(operationResult);
        ArgumentNullException.ThrowIfNull(onFailure);

        return operationResult
            .Match()
            .Failure(onFailure);
    }

    /// <summary>
    /// Asynchronously applies the specified function if the result is a failure one.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="operationResult">The operation to act on.</param>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="operationResult"/> 
    /// or <paramref name="onFailure"/> is null.</exception>
    public static async ValueTask<IOperationResult<TResult>> FailureAsync<TResult>(
        this ValueTask<IOperationResult<TResult>> operationResult,
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TResult>>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(operationResult);
        ArgumentNullException.ThrowIfNull(onFailure);

        return await (await operationResult.ConfigureAwait(false))
            .Match()
            .FailureAsync(onFailure)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously applies the specified function if the result is a failure one.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="operation"/> 
    /// or <paramref name="onFailure"/> is null.</exception>
    public static async ValueTask<IOperationResult> FailureAsync(
        this ValueTask<IOperationResult> operation,
        Func<IOperationResult, ValueTask<IOperationResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(onFailure);

        return await (await operation.ConfigureAwait(false))
            .Match()
            .FailureAsync(onFailure)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously applies the specified function if the result is a success one.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="operation"/> 
    /// or <paramref name="onSuccess"/> is null.</exception>
    public static async ValueTask<IOperationResult> SuccessAsync(
        this ValueTask<IOperationResult> operation,
        Func<IOperationResult, ValueTask<IOperationResult>> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return await (await operation.ConfigureAwait(false))
            .Match()
            .SuccessAsync(onSuccess)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously applies the specified action if the result is a success one.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="operation">The operation to act on.</param>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="operation"/> 
    /// or <paramref name="onSuccess"/> is null.</exception>
    public static async ValueTask<IOperationResult<TResult>> SuccessAsync<TResult>(
        this ValueTask<IOperationResult<TResult>> operation,
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TResult>>> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return await (await operation.ConfigureAwait(false))
            .Match()
            .SuccessAsync(onSuccess)
            .ConfigureAwait(false);
    }
}
