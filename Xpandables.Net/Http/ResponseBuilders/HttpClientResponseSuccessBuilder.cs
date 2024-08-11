
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
/// Builds the success response of <see cref="HttpClientResponse"/> type.
/// </summary>
public sealed class HttpClientResponseSuccessBuilder :
    HttpClientResponseBuilder<HttpClientResponse>
{
    ///<inheritdoc/>
    public override Type Type => typeof(HttpClientResponse);

    ///<inheritdoc/>
    public override bool CanBuild(
        Type targetType,
        HttpStatusCode targetStatusCode)
        => Type == targetType
            && targetStatusCode.IsSuccessStatusCode();

    ///<inheritdoc/>
    public async override Task<HttpClientResponse> BuildAsync(
        HttpClientResponseContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        await Task.Yield();

        if (context.ResponseMessage.Content.Headers.ContentDisposition is not null)
        {
            if (context.ResponseMessage
                .Content
                .Headers
                .ContentDisposition
                .DispositionType
                .StartsWith("attachment", StringComparison.InvariantCulture))
            {
                string fileName = context.ResponseMessage
                    .Content
                    .Headers
                    .ContentDisposition
                    .FileName!
                    .Trim('"');

                Uri requestUri = context.ResponseMessage.RequestMessage!.RequestUri!;
                string baseUrl = requestUri.GetLeftPart(UriPartial.Authority);

                string fileUrl = $"{baseUrl}/{Uri.EscapeDataString(fileName)}";

                System.Collections.Specialized.NameValueCollection headers
                    = context.ResponseMessage.ReadHttpResponseHeaders();
                headers.Add("Location", fileUrl);

                return new HttpClientResponse(
                    context.ResponseMessage.StatusCode,
                    headers,
                    context.ResponseMessage.Version,
                    context.ResponseMessage.ReasonPhrase);
            }
        }

        return new HttpClientResponse(
            context.ResponseMessage.StatusCode,
            context.ResponseMessage.ReadHttpResponseHeaders(),
            context.ResponseMessage.Version,
            context.ResponseMessage.ReasonPhrase,
            default);
    }
}
