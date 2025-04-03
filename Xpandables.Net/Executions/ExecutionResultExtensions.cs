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
using System.Reflection;

namespace Xpandables.Net.Executions;

/// <summary>
/// Provides extension methods for operation results.
/// </summary>
public static partial class ExecutionResultExtensions
{
    private const int _minSuccessStatusCode = 200;
    private const int _maxSuccessStatusCode = 299;

    private static readonly MethodInfo ToExecutionResultMethod =
        typeof(ExecutionResultExtensions).GetMethod(nameof(ToExecutionResult),
            BindingFlags.Static | BindingFlags.Public,
            [typeof(ExecutionResult)])!;

    /// <summary>  
    /// Converts the specified execution result to an <see cref="ExecutionResult{TResult}"/>.  
    /// </summary>  
    /// <typeparam name="TResult">The type of the result.</typeparam>  
    /// <param name="executionResult">The execution result to convert.</param>  
    /// <returns>An <see cref="ExecutionResult{TResult}"/> representing the generic
    /// execution result.</returns>  
    public static ExecutionResult<TResult> ToExecutionResult<TResult>(
        this ExecutionResult executionResult) => executionResult;

    /// <summary>
    /// Converts the current instance to a generic one with the specified type.
    /// </summary>
    /// <param name="executionResult">The current instance.</param>
    /// <param name="genericType">The underlying type.</param>
    /// <returns>A new instance of <see cref="ExecutionResult{TResult}"/>
    /// .</returns>
    public static dynamic ToExecutionResult(
        this ExecutionResult executionResult, Type genericType)
    {
        ArgumentNullException.ThrowIfNull(executionResult);
        ArgumentNullException.ThrowIfNull(genericType);

        if (executionResult.IsGeneric)
        {
            return executionResult;
        }

        return ToExecutionResultMethod
            .MakeGenericMethod(genericType)
            .Invoke(null, [executionResult])!;
    }


    /// <summary>
    /// Gets the title of the operation result based on its status code.
    /// </summary>
    /// <param name="statusCode">The status code to get the title for.</param>
    /// <returns>The title of the operation result.</returns>
    public static string GetTitle(this HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.OK => "Success",
            HttpStatusCode.Created => "Created",
            HttpStatusCode.Accepted => "Accepted",
            HttpStatusCode.NoContent => "No Content",
            HttpStatusCode.MovedPermanently => "Moved Permanently",
            HttpStatusCode.Found => "Found",
            HttpStatusCode.SeeOther => "See Other",
            HttpStatusCode.NotModified => "Not Modified",
            HttpStatusCode.TemporaryRedirect => "Temporary Redirect",
            HttpStatusCode.PermanentRedirect => "Permanent Redirect",
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
            HttpStatusCode.NotAcceptable => "Not Acceptable",
            HttpStatusCode.ProxyAuthenticationRequired => "Proxy Authentication Required",
            HttpStatusCode.RequestTimeout => "Request Timeout",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.Gone => "Gone",
            HttpStatusCode.LengthRequired => "Length Required",
            HttpStatusCode.PreconditionFailed => "Precondition Failed",
            HttpStatusCode.RequestEntityTooLarge => "Request Entity Too Large",
            HttpStatusCode.RequestUriTooLong => "Request-URI Too Long",
            HttpStatusCode.UnsupportedMediaType => "Unsupported Media Type",
            HttpStatusCode.RequestedRangeNotSatisfiable => "Requested Range Not Satisfiable",
            HttpStatusCode.ExpectationFailed => "Expectation Failed",
            HttpStatusCode.UpgradeRequired => "Upgrade Required",
            HttpStatusCode.InternalServerError => "Internal Server Error",
            HttpStatusCode.NotImplemented => "Not Implemented",
            HttpStatusCode.BadGateway => "Bad Gateway",
            HttpStatusCode.ServiceUnavailable => "Service Unavailable",
            HttpStatusCode.GatewayTimeout => "Gateway Timeout",
            HttpStatusCode.HttpVersionNotSupported => "HTTP Version Not Supported",
            HttpStatusCode.VariantAlsoNegotiates => "Variant Also Negotiates",
            HttpStatusCode.InsufficientStorage => "Insufficient Storage",
            HttpStatusCode.LoopDetected => "Loop Detected",
            HttpStatusCode.NotExtended => "Not Extended",
            HttpStatusCode.NetworkAuthenticationRequired => "Network Authentication Required",
            HttpStatusCode.PartialContent => "Partial Content",
            HttpStatusCode.MultipleChoices => "Multiple Choices",
            HttpStatusCode.UnprocessableEntity => "Unprocessable Entity",
            HttpStatusCode.Locked => "Locked",
            HttpStatusCode.FailedDependency => "Failed Dependency",
            HttpStatusCode.PreconditionRequired => "Precondition Required",
            HttpStatusCode.TooManyRequests => "Too Many Requests",
            HttpStatusCode.RequestHeaderFieldsTooLarge => "Request Header Fields Too Large",
            HttpStatusCode.UnavailableForLegalReasons => "Unavailable For Legal Reasons",
            HttpStatusCode.Continue => "Continue",
            HttpStatusCode.SwitchingProtocols => "Switching Protocols",
            HttpStatusCode.Processing => "Processing",
            HttpStatusCode.EarlyHints => "Early Hints",
            HttpStatusCode.IMUsed => "IM Used",
            HttpStatusCode.NonAuthoritativeInformation => "Non-Authoritative Information",
            HttpStatusCode.ResetContent => "Reset Content",
            HttpStatusCode.AlreadyReported => "Already Reported",
            HttpStatusCode.MisdirectedRequest => "Misdirected Request",
            HttpStatusCode.Unused => "Unused",
            HttpStatusCode.MultiStatus => "Multi-Status",
            HttpStatusCode.UseProxy => "Use Proxy",
            HttpStatusCode.PaymentRequired => "Payment Required",
            _ => "Unknown"
        };

#pragma warning disable IDE0072 // Add missing cases
    /// <summary>
    /// Gets the detail of the operation result based on its status code.
    /// </summary>
    /// <param name="statusCode">The status code to get the detail for.</param>
    /// <returns>The detail of the operation result.</returns>
    public static string GetDetail(this HttpStatusCode statusCode) =>
            statusCode switch
            {
                HttpStatusCode.InternalServerError or HttpStatusCode.Unauthorized
                => "Please refer to the errors/or contact administrator for additional details",
                _ => "Please refer to the errors property for additional details",
            };
#pragma warning restore IDE0072 // Add missing cases

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
    public static bool IsSuccessStatusCode(this ExecutionResult executionResult) =>
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
    public static bool IsFailureStatusCode(this ExecutionResult executionResult) =>
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
    public static bool IsCreated(this ExecutionResult executionResult) =>
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
    public static bool IsNotFound(this ExecutionResult executionResult) =>
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
    public static bool IsNoContent(this ExecutionResult executionResult) =>
        executionResult.StatusCode.IsNoContent();

    /// <summary>    
    /// Determines whether the result of the specified execution result is a file.    
    /// </summary>    
    /// <param name="executionResult">The execution result to check.</param>    
    /// <returns><see langword="true"/> if the result of the execution result is a file;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsResultFile(this ExecutionResult executionResult) =>
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
    public static bool IsBadRequest(this ExecutionResult executionResult) =>
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
    public static bool IsUnauthorized(this ExecutionResult executionResult) =>
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
    public static bool IsInternalServerError(this ExecutionResult executionResult) =>
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
    public static bool IsServiceUnavailable(this ExecutionResult executionResult) =>
        executionResult.StatusCode.IsServiceUnavailable();

    /// <summary>    
    /// Determines whether the specified HTTP status code is a validation problem
    /// status code.
    /// </summary>    
    /// <param name="statusCode">The HTTP status code to check.</param>    
    /// <returns><see langword="true"/> if the status code is a validation problem
    /// status code;     
    /// otherwise, <see langword="false"/>.</returns>    
    public static bool IsValidationProblemRequest(this HttpStatusCode statusCode) =>
        (int)statusCode is >= (int)HttpStatusCode.BadRequest and <= (int)HttpStatusCode.InternalServerError;
}
