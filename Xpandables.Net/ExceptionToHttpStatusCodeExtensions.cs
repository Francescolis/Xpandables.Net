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
}
