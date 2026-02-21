/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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

namespace System;

/// <summary>
/// Defines methods for mapping HTTP status codes to descriptive titles, details, and exceptions.
/// </summary>
/// <remarks>Implementations of this interface provide a way to translate HTTP status codes into user-friendly
/// messages or exceptions, which can be useful for error handling and reporting in web applications. The mapping logic
/// may vary depending on application requirements or localization needs.</remarks>
public interface IHttpStatusCodeExtension
{
	/// <summary>
	/// Retrieves a detailed description for the specified HTTP status code.
	/// </summary>
	/// <param name="httpStatusCode">The HTTP status code for which to obtain a detailed description.</param>
	/// <returns>A string containing the detailed description of the specified HTTP status code. Returns an empty string if no
	/// description is available.</returns>
	string GetDetail(HttpStatusCode httpStatusCode);

	/// <summary>
	/// Returns a short, human-readable title that describes the specified HTTP status code.
	/// </summary>
	/// <param name="httpStatusCode">The HTTP status code for which to retrieve the descriptive title.</param>
	/// <returns>A string containing the title associated with the specified HTTP status code. Returns an empty string if the status
	/// code is not recognized.</returns>
	string GetTitle(HttpStatusCode httpStatusCode);

	/// <summary>
	/// Creates an exception that corresponds to the specified HTTP status code, with a custom message and optional inner
	/// exception.
	/// </summary>
	/// <param name="httpStatusCode">The HTTP status code that determines the type of exception to create.</param>
	/// <param name="message">The message that describes the error. This value is used as the exception's message.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or null if no inner exception is specified.</param>
	/// <returns>An exception instance that represents the specified HTTP status code, containing the provided message and inner
	/// exception.</returns>
	Exception GetException(HttpStatusCode httpStatusCode, string message, Exception? innerException = null);
}

/// <summary>
/// Provides methods for mapping HTTP status codes to descriptive titles, details, and exceptions.
/// </summary>
/// <remarks>This class is typically used to translate HTTP status codes into user-friendly messages or to
/// generate exceptions based on status codes. It can be useful in web APIs or client applications that need to present
/// meaningful error information to users or handle errors programmatically.</remarks>
public class HttpStatusCodeExtension : IHttpStatusCodeExtension
{
	/// <inheritdoc/>
	public virtual string GetDetail(HttpStatusCode httpStatusCode) => httpStatusCode.Detail;
	/// <inheritdoc/>
	public virtual string GetTitle(HttpStatusCode httpStatusCode) => httpStatusCode.Title;
	/// <inheritdoc/>
	public virtual Exception GetException(HttpStatusCode httpStatusCode, string message, Exception? innerException = null) =>
		httpStatusCode.GetException(message, innerException);
}
