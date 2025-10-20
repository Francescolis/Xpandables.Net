
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
using System.Text.Json;

using Xpandables.Net.Collections;
using Xpandables.Net.Rests.Abstractions;

namespace Xpandables.Net.Rests.ResponseBuilders;

/// <summary>
/// Compose a generic content RestResponse asynchronously using the provided RestResponseContext.
/// </summary>
public sealed class RestResponseContentComposer : IRestResponseComposer
{
    /// <inheritdoc/>
    public bool CanCompose(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Message.IsSuccessStatusCode
            && context.Request.ResultType is null
            && context.Message.Content is not null
            && !context.Request.IsRequestStream;
    }

    /// <inheritdoc/>
    public async ValueTask<RestResponse> ComposeAsync(
        RestResponseContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        HttpResponseMessage response = context.Message;
        JsonSerializerOptions options = context.SerializerOptions;

        if (!CanCompose(context))
            throw new InvalidOperationException(
                $"{nameof(ComposeAsync)}: The response is not a success. " +
                $"Status code: {response.StatusCode} ({response.ReasonPhrase}).");
        try
        {
            string contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (IsBinaryContentType(contentType))
            {
                Stream stream = await response.Content
                    .ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (stream is null)
                {
                    return new RestResponse
                    {
                        StatusCode = response.StatusCode,
                        ReasonPhrase = response.ReasonPhrase,
                        Headers = response.Headers.ToElementCollection(),
                        Version = response.Version
                    };
                }

                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version,
                    Result = stream
                };
            }

            if (IsTextContentType(contentType))
            {
                string stringContent = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(stringContent))
                {
                    return new RestResponse
                    {
                        StatusCode = response.StatusCode,
                        ReasonPhrase = response.ReasonPhrase,
                        Headers = response.Headers.ToElementCollection(),
                        Version = response.Version
                    };
                }

                return new RestResponse
                {
                    StatusCode = response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToElementCollection(),
                    Version = response.Version,
                    Result = stringContent
                };
            }

            string generalContent = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Result = generalContent
            };
        }
        catch (Exception exception)
            when (exception is not ArgumentNullException)
        {
            return new RestResponse
            {
                StatusCode = response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Headers = response.Headers.ToElementCollection(),
                Version = response.Version,
                Exception = exception
            };
        }
    }

    private static bool IsBinaryContentType(string contentType) =>
        contentType.Contains("application/octet-stream", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("image/", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("audio/", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("video/", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/zip", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/x-7z-compressed", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/x-msdownload", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/vnd.ms-", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/vnd.openxmlformats-", StringComparison.OrdinalIgnoreCase);

    private static bool IsTextContentType(string contentType) =>
        contentType.Contains("text/", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/javascript", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/xhtml+xml", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("application/atom+xml", StringComparison.OrdinalIgnoreCase);
}
