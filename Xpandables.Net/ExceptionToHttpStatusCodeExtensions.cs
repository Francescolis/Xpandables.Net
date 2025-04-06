using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Security;
using System.Security.Authentication;

namespace Xpandables.Net;

/// <summary>
/// Provides extension methods to map .NET Framework exceptions to appropriate HTTP status codes.
/// </summary>
public static class ExceptionToStatusCodeExtensions
{
    /// <summary>
    /// Determines if a given HTTP status code indicates a successful response.
    /// </summary>
    /// <param name="statusCode">Represents the HTTP status code to evaluate for success.</param>
    /// <returns>Returns true if the status code is in the range of successful responses.</returns>
    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode) => (int)statusCode is >= 200 and < 300;

    /// <summary>
    /// Determines if a given HTTP status code indicates a failure.
    /// </summary>
    /// <param name="statusCode">Represents the HTTP status code to evaluate for success or failure.</param>
    /// <returns>Returns true if the status code signifies a failure, otherwise false.</returns>
    public static bool IsFailureStatusCode(this HttpStatusCode statusCode) => !IsSuccessStatusCode(statusCode);

    /// <summary>
    /// Maps a .NET Framework exception to the most appropriate HTTP status code.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>The appropriate HTTP status code for the exception.</returns>
    public static HttpStatusCode GetAppropriatStatusCode(this Exception exception) =>
        exception switch
        {
            // 400 - Bad Request (Client errors)
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentOutOfRangeException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            ValidationException => HttpStatusCode.BadRequest,
            FormatException => HttpStatusCode.BadRequest,

            // 401 - Unauthorized
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            AuthenticationException => HttpStatusCode.Unauthorized,

            // 403 - Forbidden
            SecurityException => HttpStatusCode.Forbidden,

            // 404 - Not Found
            FileNotFoundException => HttpStatusCode.NotFound,
            DirectoryNotFoundException => HttpStatusCode.NotFound,
            KeyNotFoundException => HttpStatusCode.NotFound,

            // 405 - Method Not Allowed
            NotSupportedException => HttpStatusCode.MethodNotAllowed,

            // 408 - Request Timeout
            TimeoutException => HttpStatusCode.RequestTimeout,

            // 409 - Conflict
            IOException => HttpStatusCode.Conflict,

            // 410 - Gone
            // No direct .NET exception maps well to Gone

            // 412 - Precondition Failed
            VersionNotFoundException => HttpStatusCode.PreconditionFailed,

            // 413 - Request Entity Too Large
            // No direct .NET exception maps well

            // 415 - Unsupported Media Type
            // No direct .NET exception maps well

            // 422 - Unprocessable Entity
            InvalidDataException => (HttpStatusCode)422, // Unprocessable Entity

            // 423 - Locked
            SynchronizationLockException => (HttpStatusCode)423, // Locked

            // 428 - Precondition Required
            // No direct .NET exception maps well

            // 429 - Too Many Requests
            // No direct .NET exception maps well

            // 502 - Bad Gateway
            WebException => HttpStatusCode.BadGateway,

            // 500 - Internal Server Error (Server errors - default case)
            NullReferenceException => HttpStatusCode.InternalServerError,
            StackOverflowException => HttpStatusCode.InternalServerError,
            OutOfMemoryException => HttpStatusCode.InternalServerError,
            InvalidOperationException => HttpStatusCode.InternalServerError,
            ApplicationException => HttpStatusCode.InternalServerError,

            // 501 - Not Implemented
            NotImplementedException => HttpStatusCode.NotImplemented,

            // 503 - Service Unavailable
            InvalidProgramException => HttpStatusCode.ServiceUnavailable,

            // 504 - Gateway Timeout
            TaskCanceledException => HttpStatusCode.GatewayTimeout,
            OperationCanceledException => HttpStatusCode.GatewayTimeout,

            // Default case
            _ => HttpStatusCode.InternalServerError
        };

    /// <summary>
    /// Creates an appropriate exception based on the provided HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to map to an exception.</param>
    /// <param name="message">The exception message.</param>
    /// <returns>An exception appropriate for the HTTP status code.</returns>
    public static Exception GetAppropriateException(this HttpStatusCode statusCode, string message)
        => statusCode switch
        {
            // 4xx Client Errors
            HttpStatusCode.BadRequest => new ArgumentException(message),
            HttpStatusCode.Unauthorized => new UnauthorizedAccessException(message),
            HttpStatusCode.PaymentRequired => new InvalidOperationException(message),
            HttpStatusCode.Forbidden => new SecurityException(message),
            HttpStatusCode.NotFound => new KeyNotFoundException(message),
            HttpStatusCode.MethodNotAllowed => new NotSupportedException(message),
            HttpStatusCode.NotAcceptable => new InvalidOperationException(message),
            HttpStatusCode.ProxyAuthenticationRequired => new AuthenticationException(message),
            HttpStatusCode.RequestTimeout => new TimeoutException(message),
            HttpStatusCode.Conflict => new IOException(message),
            HttpStatusCode.Gone => new InvalidOperationException(message),
            HttpStatusCode.LengthRequired => new ArgumentException(message),
            HttpStatusCode.PreconditionFailed => new VersionNotFoundException(message),
            HttpStatusCode.RequestEntityTooLarge => new InvalidDataException(message),
            HttpStatusCode.RequestUriTooLong => new UriFormatException(message),
            HttpStatusCode.UnsupportedMediaType => new InvalidOperationException(message),
            HttpStatusCode.RequestedRangeNotSatisfiable => new ArgumentOutOfRangeException(message),
            HttpStatusCode.ExpectationFailed => new InvalidOperationException(message),
            (HttpStatusCode)422 => new DataException(message), // Unprocessable Entity
            (HttpStatusCode)423 => new SynchronizationLockException(message), // Locked
            (HttpStatusCode)424 => new InvalidOperationException(message), // Failed Dependency
            (HttpStatusCode)428 => new InvalidOperationException(message), // Precondition Required
            (HttpStatusCode)429 => new InvalidOperationException(message), // Too Many Requests
            (HttpStatusCode)431 => new InvalidOperationException(message), // Request Header Fields Too Large
            (HttpStatusCode)451 => new InvalidOperationException(message), // Unavailable For Legal Reasons

            // 5xx Server Errors
            HttpStatusCode.InternalServerError => new InvalidOperationException(message),
            HttpStatusCode.NotImplemented => new NotImplementedException(message),
            HttpStatusCode.BadGateway => new WebException(message),
            HttpStatusCode.ServiceUnavailable => new InvalidOperationException(message),
            HttpStatusCode.GatewayTimeout => new TimeoutException(message),
            HttpStatusCode.HttpVersionNotSupported => new InvalidOperationException(message),
            (HttpStatusCode)506 => new InvalidOperationException(message), // Variant Also Negotiates
            (HttpStatusCode)507 => new InsufficientMemoryException(message), // Insufficient Storage
            (HttpStatusCode)508 => new InvalidOperationException(message), // Loop Detected
            (HttpStatusCode)510 => new InvalidOperationException(message), // Not Extended
            (HttpStatusCode)511 => new AuthenticationException(message), // Network Authentication Required

            // 1xx, 2xx, 3xx and default cases
            _ => new InvalidOperationException(message)
        };

}
