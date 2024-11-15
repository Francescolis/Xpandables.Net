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
public static partial class ExecutionResultExtensions
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
    /// Determines whether the status code of the specified execution result     
    /// is a success status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a success status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsSuccessStatusCode(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a failure status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a failure status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsFailureStatusCode(this HttpStatusCode statusCode) =>
        !statusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a failure status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a failure status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsFailureStatusCode(this IExecutionResult executionResult) =>
        !executionResult.StatusCode.IsSuccessStatusCode();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a Created status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a Created status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsCreated(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.Created;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a Created status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a Created status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsCreated(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsCreated();

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
    /// Determines whether the status code of the specified execution result     
    /// is a Not Found status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a Not Found status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNotFound(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsNotFound();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a No Content 
    /// status code.    
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a No Content 
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNoContent(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.NoContent;

    /// <summary>    
    /// Determines whether the status code of the specified execution result     
    /// is a No Content status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a No Content status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsNoContent(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsNoContent();

    /// <summary>    
    /// Determines whether the result of the specified execution result is a file.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the result of the execution result is a file;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsResultFile(this IExecutionResult executionResult) =>
       executionResult.Result is ResultFile;

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
    /// Determines whether the status code of the specified execution result     
    /// is a Bad Request status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a Bad Request status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsBadRequest(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsBadRequest();

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
    /// Determines whether the status code of the specified execution result  
    /// is an Unauthorized status code.  
    /// </summary>  
    /// <param name="executionResult">The execution result to check.</param>  
    /// <returns><see langword="true"/> if the status code of the execution  
    /// result is an Unauthorized status code;  
    /// otherwise, <see langword="false"/>.</returns>  
    public static bool IsUnauthorized(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsUnauthorized();

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
    /// Determines whether the status code of the specified execution result     
    /// is an Internal Server Error status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is an Internal Server Error status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsInternalServerError(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsInternalServerError();

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
    /// Determines whether the status code of the specified execution result     
    /// is a Service Unavailable status code.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the status code of the execution     
    /// result is a Service Unavailable status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsServiceUnavailable(this IExecutionResult executionResult) =>
        executionResult.StatusCode.IsServiceUnavailable();
}
