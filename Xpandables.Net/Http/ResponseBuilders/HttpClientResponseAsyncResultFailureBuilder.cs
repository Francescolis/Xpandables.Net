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
using System.Net;

using Xpandables.Net.Operations;

namespace Xpandables.Net.Http.ResponseBuilders;

/// <summary>
/// Builds the failure response from the <see cref="HttpRequestMessage"/>
/// of an <see cref="IAsyncEnumerable{T}"/> of specific type.
/// </summary>
/// <typeparam name="TResult">Type of the result.</typeparam>
public sealed class HttpClientResponseAsyncResultFailureBuilder<TResult>
     : HttpClientResponseBuilder<HttpClientResponse<IAsyncEnumerable<TResult>>>
{
    ///<inheritdoc/>
    public override Type Type
        => typeof(HttpClientResponse<IAsyncEnumerable<TResult>>);

    ///<inheritdoc/>
    public override bool CanBuild(
        Type targetType,
        HttpStatusCode targetStatusCode)
        => Type == targetType
            && targetStatusCode.IsFailureStatusCode();

    ///<inheritdoc/>
    public override async Task<HttpClientResponse<IAsyncEnumerable<TResult>>> BuildAsync(
        HttpClientResponseContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new HttpClientResponse<IAsyncEnumerable<TResult>>(
                context.ResponseMessage.StatusCode,
                context.ResponseMessage.ReadHttpResponseHeaders(),
                default,
                context.ResponseMessage.Version,
                context.ResponseMessage.ReasonPhrase,
                await context.ResponseMessage.BuildExceptionAsync()
                .ConfigureAwait(false));
    }
}
