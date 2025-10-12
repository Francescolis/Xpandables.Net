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
