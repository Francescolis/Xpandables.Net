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
using System.Net;

namespace Xpandables.Net.Operations;
/// <summary>
/// Represents a builder for creating successful execution results.
/// </summary>
public sealed class SuccessBuilder : Builder<ISuccessBuilder>, ISuccessBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuccessBuilder"/> class 
    /// with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code indicating a 
    /// successful execution.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is not between 200 and 299.</exception>
    public SuccessBuilder(HttpStatusCode statusCode) :
        base(statusCode)
    {
        if ((int)StatusCode is not >= 200 or not <= 299)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                statusCode,
                "The status code for success must be between 200 and 299.");
        }
    }
}

/// <summary>
/// Represents a builder for creating successful execution results with a 
/// specified result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class SuccessBuilder<TResult> :
    Builder<ISuccessBuilder<TResult>, TResult>,
    ISuccessBuilder<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuccessBuilder{TResult}"/> class 
    /// with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code indicating a 
    /// successful execution.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is not between 200 and 299.</exception>
    public SuccessBuilder(HttpStatusCode statusCode) :
        base(statusCode)
    {
        if ((int)StatusCode is not >= 200 or not <= 299)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                statusCode,
                "The status code for success must be between 200 and 299.");
        }
    }
}
