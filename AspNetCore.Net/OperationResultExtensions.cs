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
using System.OperationResults;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.Net;

/// <summary>
/// Provides extension methods for the OperationResult type to facilitate integration with ASP.NET Core model validation
/// and related workflows.
/// </summary>
public static class OperationResultExtensions
{
    /// <summary>
    /// Converts the specified model state to an operation result containing validation errors and an HTTP status code.
    /// </summary>
    /// <remarks>Only model state entries with one or more errors are included in the operation result. This
    /// method is typically used to translate model validation errors into a standardized error response.</remarks>
    /// <param name="modelState">The model state dictionary containing validation errors to include in the operation result. Cannot be null.</param>
    /// <param name="statusCode">The HTTP status code to associate with the operation result. The default is BadRequest (400).</param>
    /// <returns>An operation result representing a failure, populated with validation errors from the model state and the
    /// specified HTTP status code.</returns>
    public static OperationResult ToOperationResult(
        this ModelStateDictionary modelState,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        return OperationResult
            .FailureStatus(statusCode)
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
    public static OperationResult ToOperationResult(this BadHttpRequestException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        bool isDevelopment = (Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT") ?? Environments.Development) ==
            Environments.Development;

        int startParameterNameIndex = exception.Message
            .IndexOf('"', StringComparison.InvariantCulture) + 1;

        int endParameterNameIndex = exception.Message
            .IndexOf('"', startParameterNameIndex);

        string parameterName = exception
            .Message[startParameterNameIndex..endParameterNameIndex];

        parameterName = parameterName.Trim();

        string errorMessage = exception.Message
            .Replace("\\", string.Empty, StringComparison.InvariantCulture)
            .Replace("\"", string.Empty, StringComparison.InvariantCulture);

        return OperationResult
            .BadRequest()
            .WithTitle(((HttpStatusCode)exception.StatusCode).Title)
            .WithDetail(isDevelopment ? exception.ToString() : ((HttpStatusCode)exception.StatusCode).Detail)
            .WithStatusCode((HttpStatusCode)exception.StatusCode)
            .WithError(parameterName, errorMessage)
            .Build();
    }

    /// <summary>
    /// Extensions for <see cref="OperationResult"/>.
    /// </summary>   
    extension(OperationResult operation)
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

            foreach (ElementEntry entry in operation.Errors)
            {
                foreach (string? value in entry.Values)
                {
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    modelStateDictionary.AddModelError(entry.Key, value);
                }
            }

            if (isDevelopment && operation.Exception is not null)
            {
                modelStateDictionary.AddModelError("exception", operation.Exception.ToString());
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
            return new ObjectResult(operation)
            {
                StatusCode = (int)operation.StatusCode,
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

            var title = operation.Title ?? operation.StatusCode.Title;
            var detail = operation.Detail ?? operation.StatusCode.Detail;
            var status = (int)operation.StatusCode;
            var instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}";
            var type = isDevelopment ? operation.GetType().Name : null;
            var extensions = operation.Extensions.ToDictionaryObject();

            ProblemDetails problemDetails = operation.StatusCode.IsValidationProblem
                    ? new ValidationProblemDetails(operation.ToModelStateDictionary(isDevelopment))
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
