
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
using System.ComponentModel;

namespace Xpandables.Net.Operations;

/// <summary>
///  Allows user to apply pattern matching on the <see cref="IOperationResult"/>.
/// </summary>
public interface IOperationResultMatch
{
    /// <summary>
    /// Applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    IOperationResult Success(Func<IOperationResult, IOperationResult> onSuccess);

    /// <summary>
    /// Applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    IOperationResult Failure(Func<IOperationResult, IOperationResult> onFailure);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    ValueTask<IOperationResult> SuccessAsync(Func<IOperationResult, ValueTask<IOperationResult>> onSuccess);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    ValueTask<IOperationResult> FailureAsync(Func<IOperationResult, ValueTask<IOperationResult>> onFailure);
}

/// <summary>
/// Allows user to apply pattern matching on the result.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IOperationResultMatch<TResult> : IOperationResultMatch
{
    /// <summary>
    /// Applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    IOperationResult<TResult> Success(Func<IOperationResult<TResult>, IOperationResult<TResult>> onSuccess);

    /// <summary>
    /// Applies the specified action if the result is a success one.
    /// </summary>
    /// <typeparam name="TReturn">The type of the return.</typeparam>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    IOperationResult<TReturn> Success<TReturn>(Func<IOperationResult<TResult>, IOperationResult<TReturn>> onSuccess);

    /// <summary>
    /// Applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    IOperationResult<TResult> Failure(Func<IOperationResult<TResult>, IOperationResult<TResult>> onFailure);

    /// <summary>
    /// Applies the specified action if the result is a failure one.
    /// </summary>
    /// <typeparam name="TReturn">The type of the return.</typeparam>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    IOperationResult<TReturn> Failure<TReturn>(Func<IOperationResult<TResult>, IOperationResult<TReturn>> onFailure);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    ValueTask<IOperationResult<TResult>> SuccessAsync(
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TResult>>> onSuccess);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a success one.
    /// </summary>
    /// <typeparam name="TReturn">The type of the return.</typeparam>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    ValueTask<IOperationResult<TReturn>> SuccessAsync<TReturn>(
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TReturn>>> onSuccess);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    ValueTask<IOperationResult<TResult>> FailureAsync(
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TResult>>> onFailure);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a failure one.
    /// </summary>
    /// <typeparam name="TReturn">The type of the return.</typeparam>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    ValueTask<IOperationResult<TReturn>> FailureAsync<TReturn>(
        Func<IOperationResult<TResult>, ValueTask<IOperationResult<TReturn>>> onFailure);

    /// <summary>
    /// Applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new IOperationResult Success(Func<IOperationResult, IOperationResult> onSuccess);

    /// <summary>
    /// Applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new IOperationResult Failure(Func<IOperationResult, IOperationResult> onFailure);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new ValueTask<IOperationResult> SuccessAsync(
        Func<IOperationResult, ValueTask<IOperationResult>> onSuccess);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new ValueTask<IOperationResult> FailureAsync(
        Func<IOperationResult, ValueTask<IOperationResult>> onFailure);

}