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
namespace Xpandables.Net.Http;
/// <summary>
/// Provides with an abstract handler that can be used with 
/// <see cref="HttpClient"/> to add header authorization value
/// before request execution.
/// </summary>
/// <remarks>
/// Initializes a new instance of 
/// <see cref="RestAuthorizationHandler"/> class.
/// You need to register your handler in the DI container using one of the
/// extension methods like 
/// <see langword="ConfigurePrimaryHttpMessageHandler{THandler}(IHttpClientBuilder)"/>.
/// </remarks>
public abstract class RestAuthorizationHandler : HttpClientHandler
{
    private static readonly HttpRequestOptionsKey<bool?> IsSecuredKey = new(nameof(MapRestAttribute.IsSecured));
    /// <summary>
    /// Creates an instance of System.Net.Http.HttpResponseMessage 
    /// based on the information
    /// provided in the System.Net.Http.HttpRequestMessage 
    /// as an operation that will not block.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">A cancellation 
    /// token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The token is not available. 
    /// See inner exception.</exception>
    protected sealed override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Options.TryGetValue(IsSecuredKey, out bool? isSecured) == true
            && isSecured == true)
        {
            await ApplySecurityAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }

        return await base
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// When overridden in a derived class, applies the security
    /// on the request message.
    /// </summary>
    /// <remarks>The method get called only if the request is secured.
    /// e.g. Adds an authorization header to the request.</remarks>
    /// <param name="request">The request message to apply security.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected abstract Task ApplySecurityAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken);
}