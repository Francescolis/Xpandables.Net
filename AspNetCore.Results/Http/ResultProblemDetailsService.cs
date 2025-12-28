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
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Results;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides an implementation of the IProblemDetailsService that writes problem details using a collection of
/// registered IProblemDetailsWriter instances.
/// </summary>
/// <remarks>Writers are evaluated in the order provided. The first writer that indicates it can handle the given
/// context is used to write the response. If no suitable writer is found, an exception is thrown when using WriteAsync.
/// This service is typically used to customize or extend how problem details are formatted and written in HTTP
/// responses.</remarks>
/// <param name="writers">The collection of IProblemDetailsWriter instances used to write problem details responses. Cannot be null.</param>
public sealed class ResultProblemDetailsService(IEnumerable<IProblemDetailsWriter> writers) : IProblemDetailsService
{
    private readonly IProblemDetailsWriter[] _writers = [.. writers];

    /// <inheritdoc/>
    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        if (!await TryWriteAsync(context).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Unable to find a registered `IProblemDetailsWriter` that can write to the given context.");
        }
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ProblemDetails);
        ArgumentNullException.ThrowIfNull(context.HttpContext);

        if (context.Exception is not null)
        {
            var result = context.Exception.ToResult();
            context.ProblemDetails = result.ToProblemDetails(context.HttpContext);
        }
        else
        {
            bool isDevelopment = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
            HttpStatusCode statusCode = (HttpStatusCode?)context.ProblemDetails.Status ?? HttpStatusCode.BadRequest;
            context.ProblemDetails.Status ??= (int)statusCode;
            context.ProblemDetails.Title ??= statusCode.Title;
            context.ProblemDetails.Detail ??= statusCode.Detail;
            context.ProblemDetails.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString.Value}";
            context.ProblemDetails.Type ??= (isDevelopment ? (statusCode.IsValidationProblem ? nameof(ValidationException) : nameof(InvalidOperationException)) : null);
        }

        for (var i = 0; i < _writers.Length; i++)
        {
            var selectedWriter = _writers[i];
            if (selectedWriter.CanWrite(context))
            {
                await selectedWriter.WriteAsync(context).ConfigureAwait(false);
                return true;
            }
        }

        return false;
    }
}
