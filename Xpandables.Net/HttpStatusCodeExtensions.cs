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
using System.Data;
using System.Net;
using System.Security;
using System.Security.Authentication;

namespace Xpandables.Net;

/// <summary>
/// Provides extension methods for working with HTTP status codes.
/// </summary>
/// <remarks>This class contains static methods that extend the functionality of the <see
/// cref="HttpStatusCode"/> enumeration, enabling additional operations and convenience methods when handling
/// HTTP responses.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class HttpStatusCodeExtensions
{
    /// <summary>
    /// Provides extension methods for working with HTTP status codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to evaluate.</param>
    extension(HttpStatusCode statusCode)
    {
        /// <summary>
        /// Determines whether the specified HTTP status code represents a successful response.
        /// </summary>
        /// <returns><see langword="true"/>if the status code is in the range 200-299; otherwise, <see langword="false"/>.</returns>
        public bool IsSuccess => ((int)statusCode >= 200) && ((int)statusCode <= 299);

        /// <summary>
        /// Determines whether the current status code represents a failure condition.
        /// </summary>
        /// <returns><see langword="true"/> if the status code indicates a failure; otherwise, <see langword="false"/>.</returns>
        public bool IsFailure => !statusCode.IsSuccess;

        /// <summary>
        /// Determines whether the specified HTTP status code is OK (200).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is OK; otherwise, <see langword="false"/>.</returns>
        public bool IsOk => statusCode == HttpStatusCode.OK;

        /// <summary>
        /// Determines whether the specified HTTP status code is Created (201).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Created; otherwise, <see langword="false"/>.</returns>
        public bool IsCreated => statusCode == HttpStatusCode.Created;

        /// <summary>
        /// Determines whether the specified HTTP status code is Accepted (202).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Accepted; otherwise, <see langword="false"/>.</returns>
        public bool IsAccepted => statusCode == HttpStatusCode.Accepted;

        /// <summary>
        /// Determines whether the specified HTTP status code is No Content (204).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is No Content; otherwise, <see langword="false"/>.</returns>
        public bool IsNoContent => statusCode == HttpStatusCode.NoContent;

        /// <summary>
        /// Determines whether the specified HTTP status code is Bad Request (400).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Bad Request; otherwise, <see langword="false"/>.</returns>
        public bool IsBadRequest => statusCode == HttpStatusCode.BadRequest;

        /// <summary>
        /// Determines whether the specified HTTP status code is Unauthorized (401).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Unauthorized; otherwise, <see langword="false"/>.</returns>
        public bool IsUnauthorized => statusCode == HttpStatusCode.Unauthorized;

        /// <summary>
        /// Determines whether the specified HTTP status code is Forbidden (403).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Forbidden; otherwise, <see langword="false"/>.</returns>
        public bool IsForbidden => statusCode == HttpStatusCode.Forbidden;

        /// <summary>
        /// Determines whether the specified HTTP status code is Not Found (404).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Not Found; otherwise, <see langword="false"/>.</returns>
        public bool IsNotFound => statusCode == HttpStatusCode.NotFound;

        /// <summary>
        /// Determines whether the specified HTTP status code is Internal Server Error (500).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Internal Server Error; otherwise, <see langword="false"/>.</returns>
        public bool IsInternalServerError => statusCode == HttpStatusCode.InternalServerError;

        /// <summary>
        /// Determines whether the specified HTTP status code is Not Implemented (501).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Not Implemented; otherwise, <see langword="false"/>.</returns>
        public bool IsNotImplemented => statusCode == HttpStatusCode.NotImplemented;

        /// <summary>
        /// Determines whether the specified HTTP status code is Bad Gateway (502).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Bad Gateway; otherwise, <see langword="false"/>.</returns>
        public bool IsBadGateway => statusCode == HttpStatusCode.BadGateway;

        /// <summary>
        /// Determines whether the specified HTTP status code is Service Unavailable (503).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Service Unavailable; otherwise, <see langword="false"/>.</returns>
        public bool IsServiceUnavailable => statusCode == HttpStatusCode.ServiceUnavailable;

        /// <summary>
        /// Determines whether the specified HTTP status code is Gateway Timeout (504).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Gateway Timeout; otherwise, <see langword="false"/>.</returns>
        public bool IsGatewayTimeout => statusCode == HttpStatusCode.GatewayTimeout;

        /// <summary>
        /// Determines whether the specified HTTP status code is HTTP Version Not Supported (505).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is HTTP Version Not Supported; otherwise, <see langword="false"/>.</returns>
        public bool IsHttpVersionNotSupported => statusCode == HttpStatusCode.HttpVersionNotSupported;

        /// <summary>
        /// Determines whether the specified HTTP status code is Too Many Requests (429).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Too Many Requests; otherwise, <see langword="false"/>.</returns>
        public bool IsTooManyRequests => statusCode == (HttpStatusCode)429;

        /// <summary>
        /// Determines whether the specified HTTP status code is Conflict (409).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Conflict; otherwise, <see langword="false"/>.</returns>
        public bool IsConflict => statusCode == HttpStatusCode.Conflict;

        /// <summary>
        /// Determines whether the specified HTTP status code is Precondition Failed (412).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Precondition Failed; otherwise, <see langword="false"/>.</returns>
        public bool IsPreconditionFailed => statusCode == HttpStatusCode.PreconditionFailed;

        /// <summary>
        /// Determines whether the specified HTTP status code is Unsupported Media Type (415).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Unsupported Media Type; otherwise, <see langword="false"/>.</returns>
        public bool IsUnsupportedMediaType => statusCode == HttpStatusCode.UnsupportedMediaType;

        /// <summary>
        /// Determines whether the specified HTTP status code is Request Timeout (408).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Request Timeout; otherwise, <see langword="false"/>.</returns>
        public bool IsRequestTimeout => statusCode == HttpStatusCode.RequestTimeout;

        /// <summary>
        /// Determines whether the specified HTTP status code is Method Not Allowed (405).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Method Not Allowed; otherwise, <see langword="false"/>.</returns>
        public bool IsMethodNotAllowed => statusCode == HttpStatusCode.MethodNotAllowed;

        /// <summary>
        /// Determines whether the specified HTTP status code is Length Required (411).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Length Required; otherwise, <see langword="false"/>.</returns>
        public bool IsLengthRequired => statusCode == HttpStatusCode.LengthRequired;

        /// <summary>
        /// Determines whether the specified HTTP status code is Request Entity Too Large (413).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Request Entity Too Large; otherwise, <see langword="false"/>.</returns>
        public bool IsRequestEntityTooLarge => statusCode == HttpStatusCode.RequestEntityTooLarge;

        /// <summary>
        /// Determines whether the specified HTTP status code is Request URI Too Long (414).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Request URI Too Long; otherwise, <see langword="false"/>.</returns>
        public bool IsRequestUriTooLong => statusCode == HttpStatusCode.RequestUriTooLong;

        /// <summary>
        /// Determines whether the specified HTTP status code is Requested Range Not Satisfiable (416).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Requested Range Not Satisfiable; otherwise, <see langword="false"/>.</returns>
        public bool IsRequestedRangeNotSatisfiable => statusCode == HttpStatusCode.RequestedRangeNotSatisfiable;

        /// <summary>
        /// Determines whether the specified HTTP status code is Expectation Failed (417).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Expectation Failed; otherwise, <see langword="false"/>.</returns>
        public bool IsExpectationFailed => statusCode == HttpStatusCode.ExpectationFailed;

        /// <summary>
        /// Determines whether the specified HTTP status code is Upgrade Required (426).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Upgrade Required; otherwise, <see langword="false"/>.</returns>
        public bool IsUpgradeRequired => statusCode == HttpStatusCode.UpgradeRequired;

        /// <summary>
        /// Determines whether the specified HTTP status code is Precondition Required (428).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Precondition Required; otherwise, <see langword="false"/>.</returns>
        public bool IsPreconditionRequired => statusCode == (HttpStatusCode)428;

        /// <summary>
        /// Determines whether the specified HTTP status code is Too Many Requests (429).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Too Many Requests; otherwise, <see langword="false"/>.</returns>
        public bool IsTooManyRequestsOfficial => statusCode == (HttpStatusCode)429;

        /// <summary>
        /// Determines whether the specified HTTP status code is Request Header Fields Too Large (431).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Request Header Fields Too Large; otherwise, <see langword="false"/>.</returns>
        public bool IsRequestHeaderFieldsTooLarge => statusCode == (HttpStatusCode)431;

        /// <summary>
        /// Determines whether the specified HTTP status code is Unavailable For Legal Reasons (451).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Unavailable For Legal Reasons; otherwise, <see langword="false"/>.</returns>
        public bool IsUnavailableForLegalReasons => statusCode == (HttpStatusCode)451;

        /// <summary>
        /// Determines whether the specified HTTP status code is Network Authentication Required (511).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is Network Authentication Required; otherwise, <see langword="false"/>.</returns>
        public bool IsNetworkAuthenticationRequired => statusCode == (HttpStatusCode)511;

        /// <summary>
        /// Determines whether the specified HTTP status code represents a redirect response (3xx).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is in the range 300-399; otherwise, <see langword="false"/>.</returns>
        public bool IsRedirect => ((int)statusCode >= 300) && ((int)statusCode <= 399);

        /// <summary>
        /// Determines whether the specified HTTP status code represents a client error (4xx).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is in the range 400-499; otherwise, <see langword="false"/>.</returns>
        public bool IsClientError => ((int)statusCode >= 400) && ((int)statusCode <= 499);

        /// <summary>
        /// Determines whether the specified HTTP status code represents a server error (5xx).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is in the range 500-599; otherwise, <see langword="false"/>.</returns>
        public bool IsServerError => ((int)statusCode >= 500) && ((int)statusCode <= 599);

        /// <summary>
        /// Determines whether the specified HTTP status code represents an informational response (1xx).
        /// </summary>
        /// <returns><see langword="true"/> if the status code is in the range 100-199; otherwise, <see langword="false"/>.</returns>
        public bool IsInformational => ((int)statusCode >= 100) && ((int)statusCode <= 199);

        /// <summary>
        /// Determines whether the specified HTTP status code represents a validation problem (4xx client errors).
        /// </summary>
        /// <returns><see langword="true"/> if the status code indicates a validation problem; otherwise, <see langword="false"/>.</returns>
        public bool IsValidationProblem => (int)statusCode is >= (int)HttpStatusCode.BadRequest and < (int)HttpStatusCode.InternalServerError;

        /// <summary>
        /// Throws an exception if the HTTP status code represents a failure.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the HTTP status code indicates a failure.</exception>
        public void EnsureSuccess()
        {
            if (statusCode.IsFailure)
            {
                throw new InvalidOperationException($"The HTTP status code '{statusCode}' indicates a failure.");
            }
        }

        /// <summary>
        /// Throws an exception if the HTTP status code represents a successful response.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the HTTP status code indicates success.</exception>
        public void EnsureFailure()
        {
            if (statusCode.IsSuccess)
            {
                throw new InvalidOperationException($"The HTTP status code '{statusCode}' indicates a success.");
            }
        }

        /// <summary>
        /// Gets a user-facing message that provides additional detail about the current HTTP status code.
        /// </summary>
        public string Detail => statusCode switch
        {
            HttpStatusCode.InternalServerError or HttpStatusCode.Unauthorized
                => "Please refer to the errors/or contact administrator for additional details",
            _ => "Please refer to the errors property for additional details",
        };

        /// <summary>
        /// Gets the standard reason phrase associated with the HTTP status code for the response.
        /// </summary>
        /// <remarks>The value corresponds to the official reason phrase defined for the HTTP status code,
        /// such as "Not Found" for 404 or "OK" for 200. If the status code is not recognized, the value is
        /// "Unknown".</remarks>
        public string Title => statusCode switch
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
        /// Maps the current HTTP status code to an appropriate .NET exception instance with the specified message.
        /// </summary>
        /// <remarks>The returned exception type depends on the HTTP status code associated with the
        /// current context. For example, a 404 Not Found status returns a KeyNotFoundException, while a 401
        /// Unauthorized returns an UnauthorizedAccessException. For status codes not explicitly handled, an
        /// InvalidOperationException is returned.</remarks>
        /// <param name="message">The message to include in the created exception. Cannot be null.</param>
        /// <returns>An exception instance corresponding to the current HTTP status code, initialized with the specified message.</returns>
        public Exception GetException(string message)
        {
            ArgumentNullException.ThrowIfNull(message);

            return statusCode switch
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
                HttpStatusCode.RequestedRangeNotSatisfiable => new ArgumentOutOfRangeException(message: message, innerException: null),
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
    }
}
