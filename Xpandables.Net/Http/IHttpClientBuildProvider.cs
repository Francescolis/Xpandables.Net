
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
namespace Xpandables.Net.Http;

/// <summary>
/// Defines a contract for a builder manager that provides 
/// <see cref="IHttpClientRequestBuilder"/> and <see cref="IHttpClientResponseBuilder"/> instances.
/// </summary>
public interface IHttpClientBuildProvider
{
    /// <summary>
    /// Gets the <see cref="IHttpClientRequestBuilder"/> instance.
    /// </summary>
    IHttpClientRequestBuilder RequestBuilder { get; }

    /// <summary>
    /// Gets the <see cref="IHttpClientResponseBuilder"/> instance.
    /// </summary>
    IHttpClientResponseBuilder ResponseBuilder { get; }
}

internal sealed class HttpClientBuildProviderInternal(
    IHttpClientResponseBuilder responseBuilder,
    IHttpClientRequestBuilder requestBuilder) : IHttpClientBuildProvider
{
    private readonly IHttpClientResponseBuilder _responseBuilder = responseBuilder
        ?? throw new ArgumentNullException(nameof(responseBuilder));
    private readonly IHttpClientRequestBuilder _requestBuilder = requestBuilder
        ?? throw new ArgumentNullException(nameof(requestBuilder));

    public IHttpClientRequestBuilder RequestBuilder => _requestBuilder;

    public IHttpClientResponseBuilder ResponseBuilder => _responseBuilder;
}