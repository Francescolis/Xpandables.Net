
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
using System.ComponentModel.DataAnnotations;
using System.Net;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Xpandables.Net.Collections;
using Xpandables.Net.Extensions;
using Xpandables.Net.I18n;

namespace Xpandables.Net.Operations;
internal static class MiddlewareExtensions
{
    internal static async Task OnBadHttpExceptionAsync(
        HttpContext context,
        BadHttpRequestException exception,
        OperationResultController? controller = default)
    {
        var startParameterNameIndex = exception.Message.IndexOf('"', StringComparison.InvariantCulture) + 1;
        var endParameterNameIndex = exception.Message.IndexOf('"', startParameterNameIndex);

        var parameterName = exception.Message[startParameterNameIndex..endParameterNameIndex];
        parameterName = parameterName.Split(" ")[1].Trim();

        var errorMessage = exception.Message
            .Replace("\\", string.Empty, StringComparison.InvariantCulture)
            .Replace("\"", string.Empty, StringComparison.InvariantCulture);

        OperationResult operationResult = OperationResults
              .BadRequest()
              .WithDetail(exception.Message)
              .WithStatusCode((HttpStatusCode)exception.StatusCode)
              .WithError(parameterName, errorMessage)
              .Build();

        if (controller is not null)
        {
            var validationProblem = GetControllerValidationProblemDetails(controller, context, operationResult);
            await validationProblem.ExecuteResultAsync(controller.ControllerContext).ConfigureAwait(false);
        }
        else
        {
            await OnOperationResultAsync(context, operationResult).ConfigureAwait(false);
        }
    }

    internal static async Task OnOperationResultExceptionAsync(
        HttpContext context,
        OperationResultException exception,
        OperationResultController? controller = default)
    {
        if (controller is not null)
        {
            var validationProblem = GetControllerValidationProblemDetails(controller, context, exception.OperationResult);
            await validationProblem.ExecuteResultAsync(controller.ControllerContext).ConfigureAwait(false);
        }
        else
        {
            await OnOperationResultAsync(context, exception.OperationResult).ConfigureAwait(false);
        }
    }

    internal static async Task OnValidationExceptionAsync(
        HttpContext context,
        ValidationException exception,
        OperationResultController? controller = default)
    {
        var operationResult = exception.ValidationResult.ToOperationResult();

        if (controller is not null)
        {
            var validationProblem = GetControllerValidationProblemDetails(controller, context, operationResult);
            await validationProblem.ExecuteResultAsync(controller.ControllerContext).ConfigureAwait(false);
        }
        else
        {
            await OnOperationResultAsync(context, operationResult).ConfigureAwait(false);
        }
    }


    internal static async Task OnOperationResultAsync(HttpContext context, IOperationResult operationResult)
    {
        var minimalResult = operationResult.ToMinimalResult();

        await minimalResult.ExecuteAsync(context).ConfigureAwait(false);
    }

    internal static async Task OnExceptionAsync(
        HttpContext context,
        Exception exception,
        OperationResultController? controller = default)
    {
        if (exception is UnauthorizedAccessException && await context.GetAuthenticateSchemeAsync().ConfigureAwait(false) is { } scheme)
            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, scheme);

        if (controller is not null)
        {
            var problemDetails = GetControllerProblemDetails(controller, context, exception);
            await problemDetails.ExecuteResultAsync(controller.ControllerContext).ConfigureAwait(false);
        }
        else
        {
            var problemDetails = context.GetProblemDetails(exception);
            await problemDetails.ExecuteAsync(context).ConfigureAwait(false);
        }
    }

    internal static ActionResult GetControllerValidationProblemDetails(
        OperationResultController controller,
        HttpContext context,
        IOperationResult operationResult)
    {
        var statusCode = operationResult.StatusCode;

        var validationProblemDetails = controller.ValidationProblem(
            operationResult.GetProblemDetail(),
            context.Request.Path,
            (int)statusCode,
            operationResult.GetProblemTitle(),
            modelStateDictionary: operationResult.Errors.ToModelStateDictionary());

        return validationProblemDetails;
    }

    internal static ObjectResult GetControllerProblemDetails(
        OperationResultController controller,
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception is UnauthorizedAccessException ? HttpStatusCode.Unauthorized : HttpStatusCode.InternalServerError;

        var problemDetails = controller.Problem(
            (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development" ? $"{exception}" : statusCode.GetProblemDetail(),
            context.Request.Path,
            (int)statusCode,
            statusCode.GetProblemTitle());

        return problemDetails;
    }

    internal static OperationResultController GetExceptionController(HttpContext context)
    {
        var controller = context.RequestServices.GetRequiredService<OperationResultController>();
        controller.ControllerContext = new ControllerContext(
            new ActionContext(
                context,
                context.GetRouteData(),
                new ControllerActionDescriptor()));

        return controller;
    }

    internal static IResult GetProblemDetails(this HttpContext context, Exception exception)
    {
        if (exception is ValidationException validation)
        {
            return validation.ValidationResult.ToOperationResult().GetValidationProblemDetails(context);
        }

        if (exception is OperationResultException operationResultException)
        {
            return operationResultException.OperationResult.ToMinimalResult();
        }

        var statusCode = exception is UnauthorizedAccessException ? HttpStatusCode.Unauthorized : HttpStatusCode.InternalServerError;

        var problemDetails = Results.Problem(
            (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development") == "Development" ? $"{exception}" : statusCode.GetProblemDetail(),
            context.Request.Path,
            (int)statusCode,
            statusCode.GetProblemTitle());

        return problemDetails;
    }

    internal static IResult GetValidationProblemDetails(this IOperationResult operationResult, HttpContext context)
    {
        var statusCode = operationResult.StatusCode;

        var validationProblemDetails = Results.ValidationProblem(
            operationResult.Errors.ToMinimalErrors(),
            operationResult.GetProblemDetail(),
            context.Request.Path,
            (int)statusCode,
           operationResult.GetProblemTitle());

        return validationProblemDetails;
    }

    internal static string GetProblemDetail(this HttpStatusCode statusCode)
         => statusCode switch
         {
             HttpStatusCode.InternalServerError or HttpStatusCode.Unauthorized => I18nXpandables.HttpStatusCodeProblemDetailInternalError,
             _ => I18nXpandables.HttpStatusCodeProblemDetailPropertyError
         };

    internal static string GetProblemTitle(this HttpStatusCode statusCode)
         => statusCode switch
         {
             HttpStatusCode.NotFound => I18nXpandables.HttpStatusCodeNotFound,
             HttpStatusCode.Locked => I18nXpandables.HttpStatusCodeLocked,
             HttpStatusCode.Conflict => I18nXpandables.HttpStatusCodeConflict,
             HttpStatusCode.Unauthorized => I18nXpandables.HttpStatusCodeUnauthorized,
             HttpStatusCode.Forbidden => I18nXpandables.HttpStatusCodeForbidden,
             HttpStatusCode.InternalServerError => I18nXpandables.HttpStatusCodeInternalServerError,
             _ => I18nXpandables.HttpStatusCodeRequestValidation
         };

    internal static async Task<string?> GetAuthenticateSchemeAsync(this HttpContext context)
    {
        if (context.RequestServices.GetService<IAuthenticationSchemeProvider>() is { } authenticationSchemeProvider)
        {
            var requestSchemes = await authenticationSchemeProvider.GetRequestHandlerSchemesAsync().ConfigureAwait(false);
            var defaultSchemes = await authenticationSchemeProvider.GetDefaultAuthenticateSchemeAsync().ConfigureAwait(false);
            var allSchemes = await authenticationSchemeProvider.GetAllSchemesAsync().ConfigureAwait(false);

            var scheme = requestSchemes.FirstOrDefault() ?? defaultSchemes ?? allSchemes.FirstOrDefault();
            if (scheme is not null)
                return scheme.Name;
        }

        return default;
    }

    internal static void AddLocationUrlIfAvailable(this HttpContext context, IOperationResult operation)
    {
        if (operation.LocationUrl.IsNotEmpty)
            context.Response.Headers.Location =
                new Microsoft.Extensions.Primitives.StringValues(operation.LocationUrl.Value);
    }

    internal static void AddHeadersIfAvailable(this HttpContext context, IOperationResult operation)
    {
        if (operation.Headers.Any())
        {
            foreach (var header in operation.Headers)
                context.Response.Headers.Append(
                    header.Key,
                    new Microsoft.Extensions.Primitives.StringValues(header.Values.ToArray()));
        }
    }

    internal static async Task WriteBodyIfAvailableAsync(this HttpContext context, IOperationResult operation)
    {
        if (operation.Result.IsNotEmpty)
            await context.Response.WriteAsJsonAsync(
                operation.Result.Value,
                operation.Result.GetType())
                .ConfigureAwait(false);
    }

    internal static async Task AddHeaderIfUnauthorized(this HttpContext context, IOperationResult operation)
    {
        if (operation.StatusCode == HttpStatusCode.Unauthorized
            && await context.GetAuthenticateSchemeAsync()
            .ConfigureAwait(false) is { } scheme)
            context.Response.Headers.Append(HeaderNames.WWWAuthenticate, scheme);
    }

    internal static async Task WriteFileBodyAsync(this HttpContext context, BinaryEntry file)
    {
        var result = Results.File(file.Content, file.ContentType, file.Title);

        await result.ExecuteAsync(context).ConfigureAwait(false);
    }

    internal static async Task ResultCreatedAsync(this HttpContext context, IOperationResult operation)
    {
        if (operation.LocationUrl.IsEmpty)
            throw new InvalidOperationException(I18nXpandables.CanNotBeNull
                .StringFormat("IOperationResult.LocationUrl"));

        var result = operation.Result.IsNotEmpty switch
        {
            true => Results.Created(new Uri(operation.LocationUrl.Value), operation.Result.Value),
            _ => Results.Created(new Uri(operation.LocationUrl.Value), default)
        };

        await result.ExecuteAsync(context).ConfigureAwait(false);
    }

    internal static IOperationResult ToOperationResult(
        this ModelStateDictionary modelState,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
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

}
