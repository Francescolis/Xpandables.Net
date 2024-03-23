
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
using System.Net;

namespace Xpandables.Net.Primitives;

/// <summary>
/// Provides a set of static methods for <see cref="HttpStatusCode"/> instances.
/// </summary>
public static class PrimitiveExtensions
{
    private const int _minSuccessStatusCode = 200;
    private const int _maxSuccessStatusCode = 299;

    /// <summary>
    /// Determines whether the specified status code is a success one.
    /// </summary>
    /// <param name="statusCode">The status code to act on.</param>
    /// <returns><see langword="true"/> if the status is success, 
    /// otherwise returns <see langword="false"/></returns>
    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        => (int)statusCode is >= _minSuccessStatusCode and <= _maxSuccessStatusCode;

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
}