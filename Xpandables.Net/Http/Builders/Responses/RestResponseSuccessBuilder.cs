
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

namespace Xpandables.Net.Http.Builders.Responses;

/// <summary>
/// Builds a successful REST response based on the provided context. 
/// Validates the response type and status code before creating the response.
/// </summary>
public sealed class RestResponseSuccessBuilder : IRestResponseBuilder
{
    /// <inheritdoc/>
    public Type Type => typeof(RestResponse);

    /// <inheritdoc/>
    public Task<TRestResponse> BuildAsync<TRestResponse>(
        RestResponseContext context, CancellationToken cancellationToken = default)
        where TRestResponse : RestResponseAbstract
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!CanBuild(typeof(TRestResponse), context.Message.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and success status code.",
                new NotSupportedException("Unsupported response type"));
        }

        RestResponse response = new()
        {
            StatusCode = context.Message.StatusCode,
            Headers = context.Message.Headers.ToElementCollection(),
            Version = context.Message.Version,
            ReasonPhrase = context.Message.ReasonPhrase,
        };

        return Task.FromResult((TRestResponse)(object)response);
    }

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        targetType == Type
        && statusCode.IsSuccessStatusCode();
}
