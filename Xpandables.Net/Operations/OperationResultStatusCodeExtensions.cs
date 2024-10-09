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
public static partial class OperationResultExtensions
{
    private const int _minSuccessStatusCode = 200;
    private const int _maxSuccessStatusCode = 299;

    /// <summary>    
    /// Determines whether the specified HTTP status code is a success status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a success status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode) =>
        (int)statusCode is >= _minSuccessStatusCode and <= _maxSuccessStatusCode;

    /// <summary>    
    /// Determines whether the status code of the specified operation result     
    /// is a success status code.    
    /// </summary>    
    /// <param name="operationResult">The operation result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the operation     
    /// result is a success status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsSuccessStatusCode(this IOperationResult operationResult) =>
        operationResult.StatusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a failure status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a failure status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsFailureStatusCode(this HttpStatusCode statusCode) =>
        !statusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the status code of the specified operation result     
    /// is a failure status code.    
    /// </summary>    
    /// <param name="operationResult">The operation result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the operation     
    /// result is a failure status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsFailureStatusCode(this IOperationResult operationResult) =>
        !operationResult.StatusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a Not Found 
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a Not Found 
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNotFound(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.NotFound;

    /// <summary>    
    /// Determines whether the status code of the specified operation result     
    /// is a Not Found status code.    
    /// </summary>    
    /// <param name="operationResult">The operation result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the operation     
    /// result is a Not Found status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNotFound(this IOperationResult operationResult) =>
        operationResult.StatusCode.IsNotFound();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a Bad Request 
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a Bad Request
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsBadRequest(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.BadRequest;

    /// <summary>    
    /// Determines whether the status code of the specified operation result     
    /// is a Bad Request status code.    
    /// </summary>    
    /// <param name="operationResult">The operation result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the operation     
    /// result is a Bad Request status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsBadRequest(this IOperationResult operationResult) =>
        operationResult.StatusCode.IsBadRequest();

    /// <summary>    
    /// Determines whether the specified HTTP status code is an Unauthorized   
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is an Unauthorized   
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsUnauthorized(this HttpStatusCode statusCode) =>
       statusCode == HttpStatusCode.Unauthorized;

    /// <summary>  
    /// Determines whether the status code of the specified operation result  
    /// is an Unauthorized status code.  
    /// </summary>  
    /// <param name="operationResult">The operation result to check.</param>  
    /// <returns><see langword="true"/> if the status code of the operation  
    /// result is an Unauthorized status code;  
    /// otherwise, <see langword="false"/>.</returns>  
    public static bool IsUnauthorized(this IOperationResult operationResult) =>
        operationResult.StatusCode.IsUnauthorized();

    /// <summary>    
    /// Determines whether the specified HTTP status code is an Internal 
    /// Server Error status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is an Internal 
    /// Server Error status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsInternalServerError(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.InternalServerError;

    /// <summary>    
    /// Determines whether the status code of the specified operation result     
    /// is an Internal Server Error status code.    
    /// </summary>    
    /// <param name="operationResult">The operation result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the operation     
    /// result is an Internal Server Error status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsInternalServerError(this IOperationResult operationResult) =>
        operationResult.StatusCode.IsInternalServerError();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a Service Unavailable 
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a Service Unavailable 
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsServiceUnavailable(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.ServiceUnavailable;

    /// <summary>    
    /// Determines whether the status code of the specified operation result     
    /// is a Service Unavailable status code.    
    /// </summary>    
    /// <param name="operationResult">The operation result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the operation     
    /// result is a Service Unavailable status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsServiceUnavailable(this IOperationResult operationResult) =>
        operationResult.StatusCode.IsServiceUnavailable();

    /// <summary>    
    /// Ensures that the status code of the specified operation result is a 
    /// success status code, otherwise throws an 
    /// <see cref="OperationResultException"/>.
    /// </summary>    
    /// <param name="operationResult">The operation result to check.</param>    
    /// <exception cref="OperationResultException">Thrown if the status code 
    /// of the operation result is a failure status code.</exception>
    public static void EnsureSuccessStatusCode(this IOperationResult operationResult)
    {
        if (operationResult.IsFailureStatusCode())
        {
            throw new OperationResultException(operationResult);
        }
    }
}
