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
using System.Collections.Specialized;
using System.Net.Http.Headers;

namespace Xpandables.Net.Http;
/// <summary>
/// Provides extension methods for <see cref="IHttpClientMessageFactory"/> and 
/// <see cref="HttpResponseMessage"/>.
/// </summary>
public static class HttpClientDispatcherExtensions
{
    /// <summary>
    /// Converts the <see cref="HttpResponseHeaders"/> to a 
    /// <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="response">The response to act on.</param>
    /// <returns>An instance of <see cref="NameValueCollection"/>.</returns>
    public static NameValueCollection ToNameValueCollection(
        this HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return Enumerable
                .Empty<(string Name, string Value)>()
                .Concat(
                    response.Headers
                        .SelectMany(kvp => kvp.Value
                            .Select(v => (Name: kvp.Key, Value: v))
                            )
                        )
                .Concat(
                    response.Content.Headers
                        .SelectMany(kvp => kvp.Value
                            .Select(v => (Name: kvp.Key, Value: v))
                        )
                        )
                .Aggregate(
                    seed: new NameValueCollection(),
                    func: (nvc, pair) =>
                    {
                        (string name, string value) = pair;
                        nvc.Add(name, value); return nvc;
                    },
                    resultSelector: nvc => nvc
                    );
    }

    /// <summary>
    /// Converts the <see cref="HttpHeaders"/> to a 
    /// <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="headers">The headers to act on.</param>
    /// <returns>An instance of <see cref="NameValueCollection"/>.</returns>
    public static NameValueCollection ToNameValueCollection(
        this HttpHeaders headers)
        => Enumerable
            .Empty<(string Name, string Value)>()
            .Concat(
                headers
                    .SelectMany(kvp => kvp.Value
                        .Select(v => (Name: kvp.Key, Value: v))
                        )
                    )
            .Aggregate(
                seed: new NameValueCollection(),
                func: (nvc, pair) =>
                {
                    (string name, string value) = pair;
                    nvc.Add(name, value); return nvc;
                },
                resultSelector: nvc => nvc
                );

    /// <summary>
    /// Builds an <see cref="HttpClientException"/> asynchronously from the <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="httpResponse">The HTTP response message.</param>
    /// <returns>An instance of <see cref="HttpClientException"/> if the content is not empty; otherwise, null.</returns>
    internal static async Task<HttpClientException?>
        BuildExceptionAsync(this HttpResponseMessage httpResponse)
        => await httpResponse.Content.ReadAsStringAsync()
            .ConfigureAwait(false) switch
        {
            { } content when !string.IsNullOrWhiteSpace(content)
                => new HttpClientException(content),
            _ => default
        };
}
