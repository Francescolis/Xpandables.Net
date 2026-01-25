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
using System.Collections;
using System.Net;
using System.Results;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides extension methods for the <see cref="Result"/> type to facilitate integration with ASP.NET Core model validation
/// and related workflows.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts the specified model state to a result containing validation errors and an HTTP status code.
    /// </summary>
    /// <remarks>Only model state entries with one or more errors are included in the result. This
    /// method is typically used to translate model validation errors into a standardized error response.</remarks>
    /// <param name="modelState">The model state dictionary containing validation errors to include in the result. Cannot be null.</param>
    /// <param name="statusCode">The HTTP status code to associate with the result. The default is BadRequest (400).</param>
    /// <returns>A result representing a failure, populated with validation errors from the model state and the
    /// specified HTTP status code.</returns>
    public static Result ToResult(
        this ModelStateDictionary modelState,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        return Result
            .Failure()
            .WithStatusCode(statusCode)
            .WithErrors(ElementCollection.With(
                [.. modelState
                    .Keys
                    .Where(key => modelState[key]!.Errors.Count > 0)
                    .Select(key =>
                        new ElementEntry(
                            key,
                            [.. modelState[key]!.Errors.Select(error => error.ErrorMessage)]))]))
            .Build();
    }

    /// <summary>
    /// Converts a BadHttpRequestException to an OperationResult representing a standardized HTTP bad request error
    /// response.
    /// </summary>
    /// <remarks>In development environments, the error detail will include the full exception message. In
    /// other environments, a generic error detail is provided. The resulting OperationResult includes the parameter
    /// name and error message extracted from the exception, if available.</remarks>
    /// <param name="exception">The BadHttpRequestException instance to convert. Cannot be null.</param>
    /// <returns>An OperationResult containing details about the bad HTTP request, including status code, error title, and error
    /// details.</returns>
    public static Result ToResult(this BadHttpRequestException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        bool isDevelopment = (Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT") ?? Environments.Development) ==
            Environments.Development;

        HttpStatusCode statusCode = (HttpStatusCode)exception.StatusCode;

        try
        {
            int startParameterNameIndex = exception.Message
                .IndexOf('"', StringComparison.InvariantCulture) + 1;

            int endParameterNameIndex = exception.Message
                .IndexOf('"', startParameterNameIndex);

            if (startParameterNameIndex <= 0 || endParameterNameIndex <= startParameterNameIndex)
            {
                // Message format doesn't match expected pattern, use fallback
                return CreateFallbackResult(exception, statusCode, isDevelopment);
            }

            string parameterName = exception
                .Message[startParameterNameIndex..endParameterNameIndex]
                .Trim();

            if (string.IsNullOrWhiteSpace(parameterName))
            {
                return CreateFallbackResult(exception, statusCode, isDevelopment);
            }

            string errorMessage = exception.Message
                .Replace("\\", string.Empty, StringComparison.InvariantCulture)
                .Replace("\"", string.Empty, StringComparison.InvariantCulture);

            return Result
                .BadRequest()
                .WithTitle(statusCode.Title)
                .WithDetail(isDevelopment ? exception.ToString() : statusCode.Detail)
                .WithStatusCode(statusCode)
                .WithError(parameterName, errorMessage)
                .Build();
        }
        catch (ArgumentOutOfRangeException)
        {
            return CreateFallbackResult(exception, statusCode, isDevelopment);
        }

        static Result CreateFallbackResult(
            BadHttpRequestException exception,
            HttpStatusCode statusCode,
            bool isDevelopment)
        {
            return Result
                .BadRequest()
                .WithTitle(statusCode.Title)
                .WithDetail(isDevelopment ? exception.ToString() : statusCode.Detail)
                .WithStatusCode(statusCode)
                .WithException(exception)
                .Build();
        }
    }

    /// <summary>
    /// Extensions for <see cref="Result"/>.
    /// </summary>   
    extension(Result result)
    {
        /// <summary>
        /// Converts the current operation result's errors to a new ModelStateDictionary instance.
        /// </summary>
        /// <param name="isDevelopment">Indicates whether the application is running in a development environment.</param>
        /// <remarks>Use this method to integrate operation result errors with ASP.NET Core model
        /// validation workflows, such as displaying validation messages in views or APIs.</remarks>
        /// <returns>A ModelStateDictionary containing all errors from the operation result. Each error is added under its
        /// associated key. The dictionary will be empty if there are no errors.</returns>
        public ModelStateDictionary ToModelStateDictionary(bool isDevelopment = false)
        {
            ModelStateDictionary modelStateDictionary = new();

            foreach (ElementEntry entry in result.Errors)
            {
                foreach (string? value in entry.Values)
                {
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    modelStateDictionary.AddModelError(entry.Key, value);
                }
            }

            if (isDevelopment && result.Exception is not null)
            {
                modelStateDictionary.AddModelError("exception", result.Exception.ToString());
            }

            return modelStateDictionary;
        }

        /// <summary>
        /// Creates an <see cref="IActionResult"/> that represents the current operation result, including its status
        /// code and content.
        /// </summary>
        /// <returns>An <see cref="ObjectResult"/> containing the operation result and its associated status code.</returns>
        public IActionResult ToActionResult()
        {
            return new ObjectResult(result)
            {
                StatusCode = (int)result.StatusCode,
            };
        }

        /// <summary>
        /// Creates a ProblemDetails instance that represents the current operation result, using information from the
        /// specified HTTP context.
        /// </summary>
        /// <remarks>In development environments, the returned ProblemDetails includes the type name for
        /// additional context. The instance property is set to the HTTP method and request path. Additional data from
        /// the operation result is included in the Extensions property.</remarks>
        /// <param name="context">The current HTTP context from which request information is obtained. Cannot be null.</param>
        /// <returns>A ProblemDetails object containing details about the operation result, including status, title, detail, and
        /// request instance information. Returns a ValidationProblemDetails object if the status code indicates a
        /// validation problem.</returns>
        public ProblemDetails ToProblemDetails(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            bool isDevelopment = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment();

            var title = result.Title ?? result.StatusCode.Title;
            var detail = result.Detail ?? result.StatusCode.Detail;
            var status = (int)result.StatusCode;
            var instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}";
            var type = isDevelopment ? result.GetType().Name : null;
            var extensions = result.Extensions.ToDictionaryObject();

            ProblemDetails problemDetails = result.StatusCode.IsValidationProblem
                    ? new ValidationProblemDetails(result.ToModelStateDictionary(isDevelopment))
                    {
                        Title = title,
                        Detail = detail,
                        Status = status,
                        Instance = instance,
                        Type = type,
                        Extensions = extensions
                    }
                    : new ProblemDetails()
                    {
                        Title = title,
                        Detail = detail,
                        Status = status,
                        Instance = instance,
                        Type = type,
                        Extensions = extensions
                    };

            return problemDetails;
        }
    }
}
