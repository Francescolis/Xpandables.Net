using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Security;
using System.Security.Authentication;

namespace Xpandables.Net.Executions;

/// <summary>
/// Provides extension methods to map .NET Framework exceptions to appropriate HTTP status codes.
/// </summary>
public static class HttpStatusCodeExtensions
{
    /// <summary>
    /// Validates that the provided HTTP status code indicates a successful response. Throws an exception if the status code
    /// is outside the success range of 200 to 299.
    /// </summary>
    /// <param name="statusCode">The HTTP status code being checked for a successful indication.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status code does not fall within the range of 200 to 299.</exception>
    public static void AssertStatusCodeIsSuccess(this HttpStatusCode statusCode)
    {
        if (IsSuccessStatusCode(statusCode))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(statusCode),
            statusCode,
            "The status code for success must be between 200 and 299.");
    }

    /// <summary>
    /// Validates that the provided HTTP status code indicates a failed response. Throws an exception if the status code
    /// indicates a successful response within the range of 200 to 299.
    /// </summary>
    /// <param name="statusCode">The HTTP status code being checked for a failed indication.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the status code falls within the range of 200 to 299,
    /// representing a successful response.</exception>
    public static void AssertStatusCodeIsFailure(this HttpStatusCode statusCode)
    {
        if (IsFailureStatusCode(statusCode))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(statusCode),
            statusCode,
            "The status code for failure must not be between 200 and 299.");
    }

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
    // ReSharper disable once MemberCanBePrivate.Global
    public static bool IsFailureStatusCode(this HttpStatusCode statusCode) => !IsSuccessStatusCode(statusCode);

    /// <summary>
    /// Maps a .NET Framework exception to the most appropriate HTTP status code.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>The appropriate HTTP status code for the exception.</returns>
    public static HttpStatusCode GetAppropriateStatusCode(this Exception exception) =>
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
            DuplicateNameException => HttpStatusCode.Conflict,

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

    /// <summary>
    /// Gets the title of the operation result based on its status code.
    /// </summary>
    /// <param name="statusCode">The status code to get the title for.</param>
    /// <returns>The title of the operation result.</returns>
    public static string GetAppropriateTitle(this HttpStatusCode statusCode) =>
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

    /// <summary>
    /// Gets the detail of the operation result based on its status code.
    /// </summary>
    /// <param name="statusCode">The status code to get the detail for.</param>
    /// <returns>The detail of the operation result.</returns>
    public static string GetAppropriateDetail(this HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.InternalServerError or HttpStatusCode.Unauthorized
                => "Please refer to the errors/or contact administrator for additional details",
            _ => "Please refer to the errors property for additional details",
        };

    /// <summary>
    /// Checks if the HTTP status code is 200 OK.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is OK; otherwise, false.</returns>
    public static bool IsOk(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.OK;

    /// <summary>
    /// Checks if the HTTP status code is 201 Created.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Created; otherwise, false.</returns>
    public static bool IsCreated(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.Created;

    /// <summary>
    /// Checks if the HTTP status code is 202 Accepted.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Accepted; otherwise, false.</returns>
    public static bool IsAccepted(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.Accepted;

    /// <summary>
    /// Checks if the HTTP status code is 204 No Content.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is NoContent; otherwise, false.</returns>
    public static bool IsNoContent(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.NoContent;

    /// <summary>
    /// Checks if the HTTP status code is 301 Moved Permanently.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Moved Permanently; otherwise, false.</returns>
    public static bool IsMovedPermanently(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.MovedPermanently;

    /// <summary>
    /// Checks if the HTTP status code is 302 Found.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Found; otherwise, false.</returns>
    public static bool IsFound(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.Found;

    /// <summary>
    /// Checks if the HTTP status code is 304 Not Modified.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Not Modified; otherwise, false.</returns>
    public static bool IsNotModified(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.NotModified;

    /// <summary>
    /// Checks if the HTTP status code is 400 Bad Request.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Bad Request; otherwise, false.</returns>
    public static bool IsBadRequest(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.BadRequest;

    /// <summary>
    /// Checks if the HTTP status code is 401 Unauthorized.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Unauthorized; otherwise, false.</returns>
    public static bool IsUnauthorized(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.Unauthorized;

    /// <summary>
    /// Checks if the HTTP status code is 403 Forbidden.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Forbidden; otherwise, false.</returns>
    public static bool IsForbidden(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.Forbidden;

    /// <summary>
    /// Checks if the HTTP status code is 404 Not Found.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Not Found; otherwise, false.</returns>
    public static bool IsNotFound(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.NotFound;

    /// <summary>
    /// Checks if the HTTP status code is 405 Method Not Allowed.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Method Not Allowed; otherwise, false.</returns>
    public static bool IsMethodNotAllowed(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.MethodNotAllowed;

    /// <summary>
    /// Checks if the HTTP status code is 409 Conflict.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Conflict; otherwise, false.</returns>
    public static bool IsConflict(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.Conflict;

    /// <summary>
    /// Checks if the HTTP status code is 500 Internal Server Error.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Internal Server Error; otherwise, false.</returns>
    public static bool IsInternalServerError(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.InternalServerError;

    /// <summary>
    /// Checks if the HTTP status code is 503 Service Unavailable.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Service Unavailable; otherwise, false.</returns>
    public static bool IsServiceUnavailable(this HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.ServiceUnavailable;

    /// <summary>
    /// Checks if the HTTP status code is 504 Gateway Timeout.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is Gateway Timeout; otherwise, false.</returns>
    public static bool IsGatewayTimeout(this HttpStatusCode statusCode) => statusCode == HttpStatusCode.GatewayTimeout;

    /// <summary>
    /// Checks if the provided status code indicates a validation problem request.
    /// </summary>
    /// <param name="statusCode">Indicates the HTTP status code to evaluate for validation issues.</param>
    /// <returns>Returns true if the status code falls within the range of client error responses.</returns>
    public static bool IsValidationProblem(this HttpStatusCode statusCode) =>
        (int)statusCode is >= (int)HttpStatusCode.BadRequest and < (int)HttpStatusCode.InternalServerError;
}