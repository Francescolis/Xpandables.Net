using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xpandables.Net.Collections;

namespace Xpandables.Net.ExecutionResults;

/// <summary>
/// Provides extension methods for the ExecutionResult type to facilitate integration with ASP.NET Core model validation
/// and related workflows.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class ExecutionResultExtensions
{
    /// <summary>
    /// Converts the specified model state to an execution result containing validation errors and an HTTP status code.
    /// </summary>
    /// <remarks>Only model state entries with one or more errors are included in the execution result. This
    /// method is typically used to translate model validation errors into a standardized error response.</remarks>
    /// <param name="modelState">The model state dictionary containing validation errors to include in the execution result. Cannot be null.</param>
    /// <param name="statusCode">The HTTP status code to associate with the execution result. The default is BadRequest (400).</param>
    /// <returns>An execution result representing a failure, populated with validation errors from the model state and the
    /// specified HTTP status code.</returns>
    public static ExecutionResult ToExecutionResult(
        this ModelStateDictionary modelState,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        return ExecutionResult
            .Failure(statusCode)
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
    /// Converts a BadHttpRequestException to an ExecutionResult representing a standardized HTTP bad request error
    /// response.
    /// </summary>
    /// <remarks>In development environments, the error detail will include the full exception message. In
    /// other environments, a generic error detail is provided. The resulting ExecutionResult includes the parameter
    /// name and error message extracted from the exception, if available.</remarks>
    /// <param name="exception">The BadHttpRequestException instance to convert. Cannot be null.</param>
    /// <returns>An ExecutionResult containing details about the bad HTTP request, including status code, error title, and error
    /// details.</returns>
    public static ExecutionResult ToExecutionResult(this BadHttpRequestException exception)
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

        return ExecutionResult
            .BadRequest()
            .WithTitle(((HttpStatusCode)exception.StatusCode).Title)
            .WithDetail(isDevelopment ? exception.Message : ((HttpStatusCode)exception.StatusCode).Detail)
            .WithStatusCode((HttpStatusCode)exception.StatusCode)
            .WithError(parameterName, errorMessage)
            .Build();
    }

    /// <summary>
    /// Extensions for <see cref="ExecutionResult"/>.
    /// </summary>   
    extension(ExecutionResult executionResult)
    {
        /// <summary>
        /// Converts the current execution result's errors to a new ModelStateDictionary instance.
        /// </summary>
        /// <remarks>Use this method to integrate execution result errors with ASP.NET Core model
        /// validation workflows, such as displaying validation messages in views or APIs.</remarks>
        /// <returns>A ModelStateDictionary containing all errors from the execution result. Each error is added under its
        /// associated key. The dictionary will be empty if there are no errors.</returns>
        public ModelStateDictionary ToModelStateDictionary()
        {
            ArgumentNullException.ThrowIfNull(executionResult);
            ModelStateDictionary modelStateDictionary = new();

            foreach (ElementEntry entry in executionResult.Errors)
            {
                foreach (string? value in entry.Values)
                {
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    modelStateDictionary.AddModelError(entry.Key, value);
                }
            }

            return modelStateDictionary;
        }

        /// <summary>
        /// Creates an <see cref="IActionResult"/> that represents the current execution result, including its status
        /// code and content.
        /// </summary>
        /// <returns>An <see cref="ObjectResult"/> containing the execution result and its associated status code.</returns>
        public IActionResult ToActionResult()
        {
            ArgumentNullException.ThrowIfNull(executionResult);
            return new ObjectResult(executionResult)
            {
                StatusCode = (int)executionResult.StatusCode,
            };
        }

        /// <summary>
        /// Converts the current execution result to a minimal API result suitable for use with ASP.NET Core endpoints.
        /// </summary>
        /// <returns>An <see cref="IResult"/> instance that represents the execution result in a format compatible with minimal
        /// APIs.</returns>
        public IResult ToMinimalResult()
        {
            ArgumentNullException.ThrowIfNull(executionResult);
            return new ExecutionResultMinimalResult(executionResult);
        }

        /// <summary>
        /// Creates a ProblemDetails instance that represents the current execution result, using information from the
        /// specified HTTP context.
        /// </summary>
        /// <remarks>In development environments, the returned ProblemDetails includes the type name for
        /// additional context. The instance property is set to the HTTP method and request path. Additional data from
        /// the execution result is included in the Extensions property.</remarks>
        /// <param name="context">The current HTTP context from which request information is obtained. Cannot be null.</param>
        /// <returns>A ProblemDetails object containing details about the execution result, including status, title, detail, and
        /// request instance information. Returns a ValidationProblemDetails object if the status code indicates a
        /// validation problem.</returns>
        public ProblemDetails ToProblemDetails(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(executionResult);
            ArgumentNullException.ThrowIfNull(context);

            bool isDevelopment = context.RequestServices
                .GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment();

            var title = executionResult.Title ?? executionResult.StatusCode.Title;
            var detail = executionResult.Detail ?? executionResult.StatusCode.Detail;
            var status = (int)executionResult.StatusCode;
            var instance = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.Value}";
            var type = isDevelopment ? executionResult.GetType().Name : null;
            var extensions = executionResult.Extensions.ToDictionaryObject();

            ProblemDetails problemDetails = executionResult.StatusCode.IsValidationProblem
                    ? new ValidationProblemDetails(executionResult.ToModelStateDictionary())
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
