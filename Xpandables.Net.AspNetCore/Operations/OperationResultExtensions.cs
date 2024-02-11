
/************************************************************************************************************
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
************************************************************************************************************/
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using System.ComponentModel.DataAnnotations;
using System.Net;

using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.Collections;
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

        return operation.Title.IsNotEmpty switch
        {
            true => operation.Title,
            _ => operation.StatusCode switch
            {
                HttpStatusCode.NotFound => I18nXpandables.HttpStatusCodeNotFound,
                HttpStatusCode.Locked => I18nXpandables.HttpStatusCodeLocked,
                HttpStatusCode.Conflict => I18nXpandables.HttpStatusCodeConflict,
                HttpStatusCode.Unauthorized => I18nXpandables.HttpStatusCodeUnauthorized,
                HttpStatusCode.Forbidden => I18nXpandables.HttpStatusCodeForbidden,
                HttpStatusCode.InternalServerError => I18nXpandables.HttpStatusCodeInternalServerError,
                _ => I18nXpandables.HttpStatusCodeRequestValidation
            }
        };
    }

    /// <summary>
    /// Gets the problem detail for the specified <see cref="HttpStatusCode"/>.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The problem detail for the specified <see cref="IOperationResult"/>.</returns>
    public static string GetProblemDetail(this IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return operation.Detail.IsNotEmpty switch
        {
            true => operation.Detail,
            _ => operation.StatusCode switch
            {
                HttpStatusCode.InternalServerError or HttpStatusCode.Unauthorized =>
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
    public static async Task<string?> GetAuthenticationSchemeAsync(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.RequestServices.GetService<IAuthenticationSchemeProvider>() is { } authenticationSchemeProvider)
        {
            IEnumerable<AuthenticationScheme> requestSchemes = await authenticationSchemeProvider
                .GetRequestHandlerSchemesAsync()
                .ConfigureAwait(false);

            AuthenticationScheme? defaultSchemes = await authenticationSchemeProvider
                .GetDefaultAuthenticateSchemeAsync()
                .ConfigureAwait(false);

            IEnumerable<AuthenticationScheme> allSchemes = await authenticationSchemeProvider
                .GetAllSchemesAsync()
                .ConfigureAwait(false);

            AuthenticationScheme? scheme = requestSchemes.FirstOrDefault() ?? defaultSchemes ?? allSchemes.FirstOrDefault();
            if (scheme is not null)
                return scheme.Name;
        }

        return default;
    }

    /// <summary>
    /// Gets the validation problem details for the specified <see cref="HttpContext"/> and <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The validation problem details for the specified <see cref="HttpContext"/> and <see cref="IOperationResult"/>.</returns>
    public static IResult? GetValidationProblemDetails(
          this HttpContext context,
         IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (operation.IsSuccess)
            return default;

        HttpStatusCode statusCode = operation.StatusCode;

        IResult validationProblemDetails = Results.ValidationProblem(
            operation.Errors.ToMinimalErrors(),
            operation.GetProblemDetail(),
            context.Request.Path,
            (int)statusCode,
           operation.GetProblemTitle());

        return validationProblemDetails;
    }

    /// <summary>
    /// Gets the problem details for the specified <see cref="HttpContext"/> and <see cref="Exception"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <param name="exception">The exception to act on.</param>
    public static async Task<IResult> GetProblemDetailsAsync(
        this HttpContext context,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is ValidationException validation)
        {
            IOperationResult operation = validation.ValidationResult
                .ToOperationResult();

            return context.GetValidationProblemDetails(operation)!;
        }

        if (exception is OperationResultException operationResultException)
        {
            return context.GetValidationProblemDetails(operationResultException.OperationResult)!;
        }

        HttpStatusCode statusCode = exception is UnauthorizedAccessException
            ? HttpStatusCode.Unauthorized
            : HttpStatusCode.InternalServerError;

        IOperationResult error = OperationResults
            .Failure(statusCode)
            .Build();

        await context
                  .AddAuthenticationSchemeIfUnauthorizedAsync(error)
                  .ConfigureAwait(false);

        IResult problemDetails = Results.Problem(
            (Environment.GetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development"
                ? $"{exception}"
                : error.GetProblemDetail(),
            context.Request.Path,
            (int)statusCode,
            error.GetProblemTitle());

        return problemDetails;
    }

    /// <summary>
    /// Gets the file result for the specified <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The file result for the specified <see cref="IOperationResult"/>.</returns>
    public static IResult? GetFileResult(this IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.Result.IsNotEmpty
            && operation.Result.Value is BinaryEntry { Content: not null } file)
        {
            return TypedResults.File(file.Content, file.ContentType, file.Title);
        }

        return default;
    }

    /// <summary>
    /// Gets the created result for the specified <see cref="IOperationResult"/>.
    /// </summary>
    /// <param name="operation">The operation to act on.</param>
    /// <returns>The created result for the specified <see cref="IOperationResult"/>.</returns>
    public static IResult? GetCreatedResultIfAvailable(this IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (operation.StatusCode is not HttpStatusCode.Created)
            return default;

        if (operation.LocationUrl.IsEmpty)
            throw new InvalidOperationException(I18nXpandables.CanNotBeNull
                .StringFormat("IOperationResult.LocationUrl"));

        IResult result = operation.Result.IsNotEmpty switch
        {
            true => TypedResults.Created(new Uri(operation.LocationUrl.Value), operation.Result.Value),
            _ => TypedResults.Created(new Uri(operation.LocationUrl.Value))
        };

        return result;
    }

    /// <summary>
    /// Adds the location URL to the response header if available.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <param name="operation">The operation to act on.</param>
    public static void AddLocationUrlIfAvailable(
         this HttpContext context,
         IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (operation.LocationUrl.IsNotEmpty)
            context.Response.Headers.Location =
                new Microsoft.Extensions.Primitives
                    .StringValues(operation.LocationUrl.Value);
    }

    /// <summary>
    /// Adds the headers to the response if available.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <param name="operation">The operation to act on.</param>
    public static void AddHeadersIfAvailable(
        this HttpContext context,
        IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (operation.Headers.Any())
        {
            foreach (ElementEntry header in operation.Headers)
                context.Response.Headers.Append(
                    header.Key,
                    new Microsoft.Extensions.Primitives.StringValues(header.Values.ToArray()));
        }
    }

    /// <summary>
    /// Adds the authentication scheme to the response header if the operation is unauthorized.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <param name="operation">The operation to act on.</param>
    public static async Task AddAuthenticationSchemeIfUnauthorizedAsync(
        this HttpContext context,
        IOperationResult operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        if (operation.StatusCode == HttpStatusCode.Unauthorized
            && await context.GetAuthenticationSchemeAsync()
                .ConfigureAwait(false) is { } scheme)
            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, scheme);
    }

    /// <summary>
    /// Returns a <see cref="ModelStateDictionary"/> from the collection of errors.
    /// </summary>
    /// <param name="errors">The collection of errors to act on.</param>
    /// <returns>A new instance of <see cref="ModelStateDictionary"/> that contains found errors.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="errors"/> is null.</exception>
    public static ModelStateDictionary ToModelStateDictionary(this ElementCollection errors)
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
    /// <returns>A collection of key and values that contains found errors.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="errors"/> is null.</exception>
    public static IDictionary<string, string[]> ToMinimalErrors(this ElementCollection errors)
    {
        Dictionary<string, string[]> modelDictionary = [];
        foreach (Primitives.ElementEntry error in errors)
        {
            List<string> messages = [.. error.Values];

            modelDictionary.Add(error.Key, [.. messages]);
        }

        return modelDictionary;
    }

    /// <summary>
    /// Converts the <see cref="ModelStateDictionary"/> to an instance of <see cref="IOperationResult"/>.
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
                                .ToReadOnlyCollection()))
                    .ToList()))
            .Build();
    }

    /// <summary>
    /// Gets the result from the specified <see cref="BadHttpRequestException"/>.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> to act on.</param>
    /// <param name="exception">The exception to act on.</param>
    public static IResult GetResultFromBadHttpException(
         this HttpContext context,
         BadHttpRequestException exception)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(exception);

        int startParameterNameIndex = exception.Message.IndexOf('"', StringComparison.InvariantCulture) + 1;
        int endParameterNameIndex = exception.Message.IndexOf('"', startParameterNameIndex);

        string parameterName = exception.Message[startParameterNameIndex..endParameterNameIndex];
        parameterName = parameterName.Split(" ")[1].Trim();

        string errorMessage = exception.Message
            .Replace("\\", string.Empty, StringComparison.InvariantCulture)
            .Replace("\"", string.Empty, StringComparison.InvariantCulture);

        IOperationResult operationResult = OperationResults
              .BadRequest()
              .WithDetail(exception.Message)
              .WithStatusCode((HttpStatusCode)exception.StatusCode)
              .WithError(parameterName, errorMessage)
              .Build();

        return context.GetValidationProblemDetails(operationResult)!;
    }


    /// <summary>
    /// Returns an instance of <see cref="ObjectResult"/> that contains the current
    /// operation as value and <see cref="IOperationResult.StatusCode"/> as status code.
    /// In that case, the response will be converted to the matching action result 
    /// by the <see cref="OperationResultControllerFilter"/>.
    /// </summary>
    /// <param name="operationResult">The current operation result to act with.</param>
    /// <returns>An implementation of <see cref="IActionResult"/> response with 
    /// <see cref="IOperationResult.StatusCode"/>.</returns>
    /// <remarks>You may derive from <see cref="OperationResultControllerFilter"/> 
    /// class to customize its behaviors.</remarks>
    public static IActionResult ToActionResult(this IOperationResult operationResult)
    {
        ArgumentNullException.ThrowIfNull(operationResult);

        return new ObjectResult(operationResult) { StatusCode = (int)operationResult.StatusCode };
    }

    /// <summary>
    /// Returns an implementation of <see cref="IResult"/> that contains 
    /// the current operation as value.
    /// </summary>
    /// <param name="operationResult">The current operation result to act with.</param>
    /// <returns>An implementation of <see cref="IResult"/> 
    /// response with <see cref="IOperationResult.StatusCode"/>.</returns>
    public static IResult ToMinimalResult(this IOperationResult operationResult)
    {
        return new OperationResultMinimal(operationResult);
    }
}
