
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using System.ComponentModel.DataAnnotations;
using System.Net;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.Operations;

/// <summary>
/// <see cref="IOperationResult"/> extensions.
/// </summary>
public static partial class OperationResultExtensions
{
#pragma warning disable IDE0072 // Add missing cases

    /// <summary>
    /// Gets the problem title for the specified <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static string GetProblemTitle(this IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return (operation.Title is not null) switch
        {
            true => operation.Title,
            _ => operation.StatusCode switch
            {
                HttpStatusCode.NotFound
                => I18nXpandables.HttpStatusCodeNotFound,
                HttpStatusCode.Locked
                => I18nXpandables.HttpStatusCodeLocked,
                HttpStatusCode.Conflict
                => I18nXpandables.HttpStatusCodeConflict,
                HttpStatusCode.Unauthorized
                => I18nXpandables.HttpStatusCodeUnauthorized,
                HttpStatusCode.Forbidden
                => I18nXpandables.HttpStatusCodeForbidden,
                HttpStatusCode.InternalServerError
                => I18nXpandables.HttpStatusCodeInternalServerError,
                _ => I18nXpandables.HttpStatusCodeRequestValidation
            }
        };
    }

    /// <summary>
    /// Determines whether the specified <see cref="IOperationResult"/> is an 
    /// operation result file.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <returns><see langword="true"/> if the specified 
    /// <see cref="IOperationResult"/> 
    /// is an operation result file; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="operation"/> 
    /// is null.</exception>"
    public static bool IsOperationResultFile(this IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return operation.Result is BinaryResult { Stream: not null };
    }

    /// <summary>
    /// Gets the problem extensions for the specified <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The problem extensions for the specified 
    /// <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="operation"/> is null.</exception>
    public static IDictionary<string, object?>? GetProblemExtensions(
        this IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (!operation.Extensions.Any())
            return default;

        Dictionary<string, object?> modelDictionary = [];
        foreach (ElementEntry extension in operation.Extensions)
        {
            string extensions = extension.Values.StringJoin(" ");

            modelDictionary.Add(extension.Key, extensions);
        }

        return modelDictionary;
    }

    /// <summary>
    /// Gets the problem detail for the specified <see cref="HttpStatusCode"/>.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The problem detail for the specified 
    /// <see cref="IOperationResult"/>.</returns>
    public static string GetProblemDetail(this IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return (operation.Detail is not null) switch
        {
            true => operation.Detail,
            _ => operation.StatusCode switch
            {
                HttpStatusCode.InternalServerError
                or HttpStatusCode.Unauthorized =>
                   I18nXpandables.HttpStatusCodeProblemDetailInternalError,
                _ => I18nXpandables.HttpStatusCodeProblemDetailPropertyError
            }
        };
    }
#pragma warning restore IDE0072 // Add missing cases

    /// <summary>
    /// Gets the authentication scheme from the current <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <returns>The authentication scheme name.</returns>
    public static async Task<string?> GetAuthenticationSchemeAsync(
        this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.RequestServices.GetService<IAuthenticationSchemeProvider>()
            is { } authenticationSchemeProvider)
        {
            IEnumerable<AuthenticationScheme> requestSchemes =
                await authenticationSchemeProvider
                .GetRequestHandlerSchemesAsync()
                .ConfigureAwait(false);

            AuthenticationScheme? defaultSchemes =
                await authenticationSchemeProvider
                .GetDefaultAuthenticateSchemeAsync()
                .ConfigureAwait(false);

            IEnumerable<AuthenticationScheme> allSchemes =
                await authenticationSchemeProvider
                .GetAllSchemesAsync()
                .ConfigureAwait(false);

            AuthenticationScheme? scheme = requestSchemes.FirstOrDefault()
                ?? defaultSchemes
                ?? allSchemes.FirstOrDefault();
            if (scheme is not null)
                return scheme.Name;
        }

        return default;
    }

    /// <summary>
    /// Builds the problem details for the specified <see cref="HttpContext"/> 
    /// and <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on
    /// .</param>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The problem details for the specified <see cref="HttpContext"/> 
    /// and <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="context"/> 
    /// or <paramref name="operation"/> is null.</exception>
    public static ProblemDetails BuildProblemDetails(
        HttpContext context,
        IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operation);

        HttpStatusCode statusCode = operation.StatusCode;
        context.Response.StatusCode = (int)statusCode;

        return new ValidationProblemDetails(operation.Errors.ToMinimalErrors())
        {
            Title = operation.GetProblemTitle(),
            Detail = operation.GetProblemDetail(),
            Status = (int)statusCode,
            Instance = $"{context.Request.Method} {context.Request.Path}",
            Type = (Environment.GetEnvironmentVariable(
                        "ASPNETCORE_ENVIRONMENT")
                            ?? "Development") == "Development"
                        ? operation.GetTypeName()
                        : default,
            Extensions = operation.GetProblemExtensions()
                ?? new Dictionary<string, object?>()
        };
    }

    /// <summary>
    /// Produces the file result for the specified <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on
    /// .</param>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The file result for the specified 
    /// <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="context"/> 
    /// or <paramref name="operation"/> is null.</exception>
    public static async ValueTask BuildFileResponseAsync(
        this HttpContext context,
        IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.Result is BinaryResult { Stream: not null } file)
        {
            Microsoft.AspNetCore.Http.HttpResults.FileStreamHttpResult result
                = TypedResults.File(file.Stream, file.ContentType, file.Name);

            await result
                .ExecuteAsync(context)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Applies the created result for the specified 
    /// <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act 
    /// on.</param>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The created result for the specified 
    /// <see cref="IOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="context"/> 
    /// or <paramref name="operation"/> is null.</exception>
    public static async ValueTask BuildCreatedResponseAsync(
        this HttpContext context,
        IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.LocationUrl is null)
            throw new InvalidOperationException(I18nXpandables.CanNotBeNull
                .StringFormat("IOperationResult.LocationUrl"));

        IResult result = (operation.Result is not null) switch
        {
            true => TypedResults.Created(operation.LocationUrl, operation.Result),
            _ => TypedResults.Created(operation.LocationUrl)
        };

        await result
            .ExecuteAsync(context)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the meta data context for the specified <see cref="HttpContext"/>
    /// and <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act 
    /// on.</param>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous 
    /// operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="context"/> 
    /// or <paramref name="operation"/> is null.</exception>
    public static async ValueTask BuildMetaDataContextAsync(
        this HttpContext context,
        IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.LocationUrl is not null)
            context.Response.Headers.Location =
                new Microsoft.Extensions.Primitives
                    .StringValues(operation.LocationUrl.ToString());

        if (operation.Headers.Any())
        {
            foreach (ElementEntry header in operation.Headers)
                context.Response.Headers.Append(
                    header.Key,
                    new Microsoft.Extensions.Primitives
                    .StringValues([.. header.Values]));
        }

        if (operation.StatusCode == HttpStatusCode.Unauthorized
                 && await context.GetAuthenticationSchemeAsync()
                     .ConfigureAwait(false) is { } scheme)
            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, scheme);
    }

    /// <summary>
    /// Builds the problem details for the specified <see cref="HttpContext"/> 
    /// and <see cref="Exception"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <param name="exception">The exception to act on.</param>
    /// <returns>The problem details for the specified <see cref="HttpContext"/> 
    /// and <see cref="Exception"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="context"/> 
    /// or <paramref name="exception"/> is null.</exception>
    public static ProblemDetails BuildProblemDetails(
        HttpContext context,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is ValidationException
            or OperationResultException
            or BadHttpRequestException)
        {
            IOperationResult operation;
            if (exception is ValidationException validation)
                operation = validation.ValidationResult.ToOperationResult();
            else if (exception is OperationResultException operationResultException)
                operation = operationResultException.Operation;
            else
            {
                BadHttpRequestException badHttpRequestException =
                    (BadHttpRequestException)exception;

                int startParameterNameIndex = badHttpRequestException.Message
                    .IndexOf('"', StringComparison.InvariantCulture) + 1;

                int endParameterNameIndex = badHttpRequestException.Message
                    .IndexOf('"', startParameterNameIndex);

                string parameterName = badHttpRequestException
                    .Message[startParameterNameIndex..endParameterNameIndex];

                parameterName = parameterName.Trim();

                string errorMessage = badHttpRequestException.Message
                    .Replace("\\", string.Empty, StringComparison.InvariantCulture)
                    .Replace("\"", string.Empty, StringComparison.InvariantCulture);

                operation = OperationResults
                      .BadRequest()
                      .WithDetail(badHttpRequestException.Message)
                      .WithStatusCode((HttpStatusCode)badHttpRequestException.StatusCode)
                      .WithError(parameterName, errorMessage)
                      .WithExtension("trace-id", context.TraceIdentifier)
                      .Build();

            }

            return BuildProblemDetails(context, operation);
        }

        HttpStatusCode statusCode = exception is UnauthorizedAccessException
            ? HttpStatusCode.Unauthorized
            : HttpStatusCode.InternalServerError;

        IOperationResult error = OperationResults
            .Failure(statusCode)
            .Build();

        return new ProblemDetails
        {
            Title = (Environment.GetEnvironmentVariable(
                        "ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development"
                        ? exception.Message
                        : error.GetProblemTitle(),
            Detail = (Environment.GetEnvironmentVariable(
                        "ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development"
                        ? $"{exception}"
                        : error.GetProblemDetail(),
            Instance = $"{context.Request.Method} {context.Request.Path}",
            Status = (int)statusCode,
            Type = (Environment.GetEnvironmentVariable(
                        "ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development"
                        ? exception.GetTypeName()
                        : default
        };
    }

    /// <summary>
    /// Gets the problem details for the specified <see cref="HttpContext"/> 
    /// and <see cref="Exception"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act 
    /// on.</param>
    /// <param name="exception">The exception to act on.</param>
    public static async ValueTask GetProblemDetailsAsync(
        this HttpContext context,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(exception);

        ProblemDetails problemDetails = BuildProblemDetails(context, exception);

        HttpStatusCode statusCode = exception is UnauthorizedAccessException
            ? HttpStatusCode.Unauthorized
            : HttpStatusCode.InternalServerError;

        IOperationResult operation = OperationResults
            .Failure(statusCode)
            .Build();

        context.Response.StatusCode = (int)statusCode;

        if (operation.StatusCode == HttpStatusCode.Unauthorized
            && await context.GetAuthenticationSchemeAsync()
                .ConfigureAwait(false) is { } scheme)
            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, scheme);

        if (context.RequestServices
            .GetService<IProblemDetailsService>() is { } problemDetailsService)
        {
            await problemDetailsService
                .WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problemDetails,
                    Exception = exception
                }).ConfigureAwait(false);

            return;
        }

        IResult result = Results.Problem(problemDetails);
        await result.ExecuteAsync(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the problem details for the specified <see cref="HttpContext"/> 
    /// and <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <param name="operation">The operation to act on.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="context"/> 
    /// or <paramref name="operation"/> is null.</exception>
    public static async ValueTask GetProblemDetailsAsync(
        this HttpContext context,
        IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operation);

        ProblemDetails problemDetails = BuildProblemDetails(context, operation);

        if (context.RequestServices
            .GetService<IProblemDetailsService>() is { } problemDetailsService)
        {
            await problemDetailsService
                .WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problemDetails,
                }).ConfigureAwait(false);

            return;
        }

        IResult result = Results.Problem(problemDetails);
        await result.ExecuteAsync(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a <see cref="ModelStateDictionary"/> from the collection 
    /// of errors.
    /// </summary>
    /// <param name="errors">The collection of errors to act on.</param>
    /// <returns>A new instance of <see cref="ModelStateDictionary"/> that 
    /// contains found errors.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="errors"/> is null.</exception>
    public static ModelStateDictionary ToModelStateDictionary(
        this ElementCollection errors)
    {
        ModelStateDictionary modelStateDictionary = new();
        foreach (Primitives.ElementEntry error in errors)
        {
            foreach (string errorMessage in error.Values)
                modelStateDictionary.AddModelError(error.Key, errorMessage);
        }

        return modelStateDictionary;
    }

    /// <summary>
    /// Returns a <see cref="IDictionary{TKey, TValue}"/> from the collection of 
    /// errors to act for errors property on <see cref="ValidationProblemDetails"/>.
    /// </summary>
    /// <param name="errors">The collection of errors to act on.</param>
    /// <returns>A collection of key and values that contains found errors
    /// .</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="errors"/> 
    /// is null.</exception>
    public static IDictionary<string, string[]> ToMinimalErrors(
        this ElementCollection errors)
    {
        Dictionary<string, string[]> modelDictionary = [];
        foreach (ElementEntry error in errors)
        {
            List<string> messages = [.. error.Values];

            modelDictionary.Add(error.Key, [.. messages]);
        }

        return modelDictionary;
    }

    /// <summary>
    /// Converts the <see cref="ModelStateDictionary"/> to an instance 
    /// of <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="modelState">The model state to act on.</param>
    /// <param name="statusCode">The status code to act on.</param>
    public static IOperationResult ToOperationResult(
     this ModelStateDictionary modelState,
     HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        _ = modelState ?? throw new ArgumentNullException(nameof(modelState));
        if (modelState.IsValid) throw new ArgumentException(
            I18nXpandables.OperationResultCanNotConvertModelState);

        return OperationResults.Failure(statusCode)
            .WithErrors(ElementCollection.With(
                modelState
                    .Keys
                    .Where(key => modelState[key]!.Errors.Count > 0)
                    .Select(key =>
                        new ElementEntry(
                            key,
                            modelState[key]!.Errors
                                .Select(error => error.ErrorMessage)
                                .ToArray()))
                    .ToList()))
            .Build();
    }

    /// <summary>
    /// Returns an instance of <see cref="ObjectResult"/> that contains the 
    /// current operation as value and <see cref="IOperationResult.StatusCode"/> 
    /// as status code. In that case, the response will be converted to the 
    /// matching action result by the 
    /// <see cref="OperationResultControllerFilter"/>.
    /// </summary>
    /// <param name="operationResult">The current operation result to act 
    /// with.</param>
    /// <returns>An implementation of <see cref="IActionResult"/> response with 
    /// <see cref="IOperationResult.StatusCode"/>.</returns>
    /// <remarks>You may derive from <see cref="OperationResultControllerFilter"/> 
    /// class to customize its behaviors.</remarks>
    public static IActionResult ToActionResult(this IOperationResult operationResult)
    {
        ArgumentNullException.ThrowIfNull(operationResult);

        return new ObjectResult(operationResult)
        {
            StatusCode =
            (int)operationResult.StatusCode
        };
    }

    /// <summary>
    /// Returns an implementation of <see cref="IResult"/> that contains 
    /// the current operation as value.
    /// </summary>
    /// <param name="operationResult">The current operation result to act 
    /// with.</param>
    /// <returns>An implementation of <see cref="IResult"/> 
    /// response with <see cref="IOperationResult.StatusCode"/>.</returns>
    public static IResult ToMinimalResult(this IOperationResult operationResult)
    {
        return new OperationResultMinimal(operationResult);
    }
}
