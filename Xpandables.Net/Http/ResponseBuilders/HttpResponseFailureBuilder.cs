﻿
/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using Xpandables.Net.Executions;

namespace Xpandables.Net.Http.ResponseBuilders;
/// <summary>
/// A builder for creating failure HTTP client responses.
/// </summary>
public sealed class HttpResponseFailureBuilder : IHttpResponseBuilder
{
    /// <inheritdoc/>
    public Type Type => typeof(HttpResponse);

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        targetType == Type
        && statusCode.IsFailureStatusCode();

    /// <inheritdoc/>
    public async Task<TResponse> BuildAsync<TResponse>(
        ResponseContext context,
        CancellationToken cancellationToken = default)
        where TResponse : HttpResponse
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!CanBuild(typeof(TResponse), context.Message.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and failure status code.",
                new NotSupportedException("Unsupported response type"));
        }

        HttpResponse response = new()
        {
            StatusCode = context.Message.StatusCode,
            Headers = context.Message.ToNameValueCollection(),
            Version = context.Message.Version,
            ReasonPhrase = context.Message.ReasonPhrase,
            Exception = await context.Message.BuildExceptionAsync().ConfigureAwait(false)
        };

        return (TResponse)response;
    }
}
