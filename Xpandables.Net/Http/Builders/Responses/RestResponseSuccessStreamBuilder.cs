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
using System.Text.Json;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Http.Builders.Responses;

/// <summary>
/// Builds a successful REST response asynchronously from a stream. 
/// Validates response type and status code before deserialization.
/// </summary>
public sealed class RestResponseSuccessStreamBuilder : IRestResponseBuilder
{
    /// <inheritdoc/>
    public Type Type => typeof(RestResponse<>);

    /// <inheritdoc/>
    public async Task<TRestResponse> BuildAsync<TRestResponse>(
        RestResponseContext context, CancellationToken cancellationToken = default)
        where TRestResponse : RestResponseAbstract
    {
        if (!CanBuild(typeof(TRestResponse), context.Message.StatusCode))
        {
            throw new InvalidOperationException(
                $"The response type must be {Type.Name} and success status code.",
                new NotSupportedException("Unsupported response type"));
        }

        Type resultType = typeof(TRestResponse).GetGenericArguments()[0];

        using Stream stream = await context.Message.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = (await JsonSerializer
            .DeserializeAsync(stream, resultType, context.SerializerOptions, cancellationToken)
            .ConfigureAwait(false));

#pragma warning disable CA2000 // Dispose objects before losing scope
        var response = new RestResponse
        {
            StatusCode = context.Message.StatusCode,
            Headers = context.Message.Headers.ToElementCollection(),
            Version = context.Message.Version,
            Result = result,
            ReasonPhrase = context.Message.ReasonPhrase,
        };
#pragma warning restore CA2000 // Dispose objects before losing scope

        if (context.Message.Content.Headers.ContentDisposition is not null)
        {
            string fileName = context.Message.Content.Headers.ContentDisposition.FileName!.Trim('"');
            Uri requestUri = context.Message.RequestMessage!.RequestUri!;
            string baseUrl = requestUri.GetLeftPart(UriPartial.Authority);
            string fileUrl = $"{baseUrl}/{Uri.EscapeDataString(fileName)}";
            ElementCollection fileCollection = ElementCollection.With("Location", fileUrl);
            response.Headers.Merge(fileCollection);
        }

        return response.ToRestResponse(resultType);
    }

    /// <inheritdoc/>
    public bool CanBuild(Type targetType, HttpStatusCode statusCode) =>
        statusCode.IsSuccessStatusCode() &&
        targetType.IsGenericType &&
        targetType.GetGenericTypeDefinition() == Type &&
        targetType.GetGenericArguments()[0].IsGenericType &&
        targetType.GetGenericArguments()[0].IsInterface &&
        targetType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>);
}
