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
/// Represents a builder for creating failure execution results.  
/// </summary>  
public sealed class FailureBuilder : Builder<IFailureBuilder>, IFailureBuilder
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="FailureBuilder"/> class 
    /// with the specified status code.  
    /// </summary>  
    /// <param name="statusCode">The HTTP status code for the failure.</param>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status 
    /// code is between 200 and 299.</exception>  
    public FailureBuilder(HttpStatusCode statusCode) :
        base(statusCode)
    {
        if ((int)StatusCode is >= 200 and <= 299)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                statusCode,
                "The status code for failure must not be between 200 and 299.");
        }
    }
}

/// <summary>  
/// Represents a builder for creating failure execution results with a specific 
/// result type.  
/// </summary>  
/// <typeparam name="TResult">The type of the result.</typeparam>  
public sealed class FailureBuilder<TResult> :
   Builder<IFailureBuilder<TResult>, TResult>,
   IFailureBuilder<TResult>
{
    /// <summary>  
    /// Initializes a new instance of the <see cref="FailureBuilder{TResult}"/> class  
    /// with the specified status code.  
    /// </summary>  
    /// <param name="statusCode">The HTTP status code for the failure.</param>  
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status  
    /// code is between 200 and 299.</exception>  
    public FailureBuilder(HttpStatusCode statusCode) :
        base(statusCode)
    {
        if ((int)StatusCode is >= 200 and <= 299)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                statusCode,
                "The status code for failure must not be between 200 and 299.");
        }
    }
}
