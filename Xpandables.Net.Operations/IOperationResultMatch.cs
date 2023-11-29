
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
    OperationResult Success(Func<OperationResult, OperationResult> onSuccess);

    /// <summary>
    /// Applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    OperationResult Failure(Func<OperationResult, OperationResult> onFailure);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    ValueTask<OperationResult> SuccessAsync(Func<OperationResult, ValueTask<OperationResult>> onSuccess);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    ValueTask<OperationResult> FailureAsync(Func<OperationResult, ValueTask<OperationResult>> onFailure);
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
    OperationResult<TResult> Success(Func<OperationResult<TResult>, OperationResult<TResult>> onSuccess);

    /// <summary>
    /// Applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    OperationResult<TResult> Failure(Func<OperationResult<TResult>, OperationResult<TResult>> onFailure);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    ValueTask<OperationResult<TResult>> SuccessAsync(Func<OperationResult<TResult>, ValueTask<OperationResult<TResult>>> onSuccess);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    ValueTask<OperationResult<TResult>> FailureAsync(Func<OperationResult<TResult>, ValueTask<OperationResult<TResult>>> onFailure);

    /// <summary>
    /// Applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new OperationResult Success(Func<OperationResult, OperationResult> onSuccess);

    /// <summary>
    /// Applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new OperationResult Failure(Func<OperationResult, OperationResult> onFailure);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a success one.
    /// </summary>
    /// <param name="onSuccess">The delegate to be used on success.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onSuccess"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new ValueTask<OperationResult> SuccessAsync(Func<OperationResult, ValueTask<OperationResult>> onSuccess);

    /// <summary>
    /// Asynchronously applies the specified action if the result is a failure one.
    /// </summary>
    /// <param name="onFailure">The delegate to be used on failure.</param>
    /// <returns>The current instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="onFailure"/> is null.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new ValueTask<OperationResult> FailureAsync(Func<OperationResult, ValueTask<OperationResult>> onFailure);

}