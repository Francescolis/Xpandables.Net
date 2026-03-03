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
using System.Reflection;
using System.Results;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides an endpoint filter that processes execution results for minimal API endpoints.
/// </summary>
/// <remarks>This filter handles responses of type <see cref="Result"/> by writing execution headers and
/// returning the underlying value. The filter is intended for use in minimal API pipelines to standardize
/// result handling and response formatting.</remarks>
public sealed class ResultEndpointFilter : IEndpointFilter
{
	private readonly ILogger _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="ResultEndpointFilter"/> class.
	/// </summary>
	/// <param name="loggerFactory">The logger factory used to create a logger with the
	/// <c>"ResultMiddleware"</c> category name.</param>
	public ResultEndpointFilter(ILoggerFactory loggerFactory)
	{
		ArgumentNullException.ThrowIfNull(loggerFactory);
		_logger = loggerFactory.CreateLogger(ResultLog.CategoryName);
	}

	/// <inheritdoc/>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "<Pending>")]
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(next);

		try
		{
			object? objectResult = await next(context).ConfigureAwait(false);

			if (objectResult is Result result)
			{
				IResultHeaderWriter headerWriter = context.HttpContext.RequestServices
					.GetRequiredService<IResultHeaderWriter>();

				await headerWriter
					.WriteAsync(context.HttpContext, result)
					.ConfigureAwait(false);

				if (result.IsFailure)
				{
					ResultLog.LogResultFailure(
						_logger,
						context.HttpContext.Request.Method,
						context.HttpContext.Request.Path,
						result.StatusCode,
						result.Exception?.ToString() ?? result.Errors.ToString());

					await WriteProblemDetailsAsync(context.HttpContext, result).ConfigureAwait(false);
					return Results.Empty;
				}

				ResultLog.LogResultCompleted(
					_logger,
					context.HttpContext.Request.Method,
					context.HttpContext.Request.Path,
					result.StatusCode);

				if (result.InternalValue is not null)
				{
					objectResult = result.InternalValue;
				}
				else
				{
					return Results.Empty;
				}
			}

			return objectResult;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception exception)
			when (!context.HttpContext.Response.HasStarted)
		{
			if (exception is TargetInvocationException targetInvocation)
			{
				exception = targetInvocation.InnerException ?? targetInvocation;
			}

			ResultLog.LogUnhandledException(
				_logger,
				context.HttpContext.Request.Method,
				context.HttpContext.Request.Path,
				exception);

			Result result = exception switch
			{
				BadHttpRequestException badHttpRequestException => badHttpRequestException.ToResult(context.HttpContext),
				ResultException executionResultException => executionResultException.Result,
				_ => exception.ToResult()
			};

			await WriteProblemDetailsAsync(context.HttpContext, result).ConfigureAwait(false);

			return Results.Empty;
		}
	}

	internal static async ValueTask WriteProblemDetailsAsync(HttpContext context, Result result)
	{
		var problem = result.ToProblemDetails(context);
		context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
		context.Response.ContentType = "application/problem+json";
		context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
		context.Response.Headers.Pragma = "no-cache";

		if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
		{
			await problemDetailsService.WriteAsync(new ProblemDetailsContext
			{
				HttpContext = context,
				ProblemDetails = problem
			}).ConfigureAwait(false);
		}
		else
		{
			IResult objectResult = Results.Problem(problem);
			await objectResult.ExecuteAsync(context).ConfigureAwait(false);
		}
	}
}
