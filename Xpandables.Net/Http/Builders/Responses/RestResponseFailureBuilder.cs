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

namespace Xpandables.Net.Http.Builders.Responses;

/// <summary>
/// Builds a RestResponse asynchronously based on the provided context and checks 
/// if the response type is valid for failure status codes.
/// </summary>
public sealed class RestResponseFailureBuilder : IRestResponseBuilder
{
    /// <inheritdoc/>
    public Type Type => typeof(RestResponseAbstract);

    /// <inheritdoc/>
    public async Task<TRestResponse> BuildAsync<TRestResponse>(
        RestResponseContext context, CancellationToken cancellationToken = default)
        where TRestResponse : RestResponseAbstract
    {
        if (!CanBuild(typeof(TRestResponse), context.Message.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and failure status code.",
                new NotSupportedException("Unsupported response type"));
        }

        string message = await context.Message.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

#pragma warning disable CA2000 // Dispose objects before losing scope
        var response = new RestResponse
        {
            StatusCode = context.Message.StatusCode,
            Headers = context.Message.Headers.ToElementCollection(),
            Version = context.Message.Version,
            ReasonPhrase = context.Message.ReasonPhrase,
            Exception = context.Message.StatusCode.GetAppropriateException(message)
        };

        dynamic result = response;
#pragma warning restore CA2000 // Dispose objects before losing scope

        if (typeof(TRestResponse).IsGenericType)
        {
            Type genericType = typeof(TRestResponse).GetGenericArguments()[0];
            result = response.ToRestResponse(genericType);
        }

        return result;
    }

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        Type.IsAssignableFrom(targetType)
        && statusCode.IsFailureStatusCode();
}
