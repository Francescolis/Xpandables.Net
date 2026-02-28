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

using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides high-performance structured log messages for the Result middleware pipeline.
/// </summary>
/// <remarks>Both <see cref="ResultMiddleware"/> and <see cref="ResultEndpointFilter"/>
/// share this class to avoid duplicating <c>[LoggerMessage]</c> definitions.
/// All messages use the <c>"ResultMiddleware"</c> category name.</remarks>
internal static partial class ResultLog
{
	internal const string CategoryName = "ResultMiddleware";

	[LoggerMessage(
		EventId = 1,
		Level = LogLevel.Warning,
		Message = "Request {Method} {Path} produced a failure result: {StatusCode} — {ErrorMessage}")]
	internal static partial void LogResultFailure(
		ILogger logger,
		string method,
		string path,
		HttpStatusCode statusCode,
		string? errorMessage);

	[LoggerMessage(
		EventId = 2,
		Level = LogLevel.Error,
		Message = "Unhandled exception in request {Method} {Path}")]
	internal static partial void LogUnhandledException(
		ILogger logger,
		string method,
		string path,
		Exception exception);

	[LoggerMessage(
		EventId = 3,
		Level = LogLevel.Debug,
		Message = "Request {Method} {Path} completed with result status {StatusCode}")]
	internal static partial void LogResultCompleted(
		ILogger logger,
		string method,
		string path,
		HttpStatusCode statusCode);
}
