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
using System.ComponentModel;

namespace Xpandables.Net.Http.Builders;

/// <summary>
/// Builds the request for <see cref="IRestClient"/>.
/// </summary>
public interface IRestRequestBuilder
{
    /// <summary>
    /// Gets the request type being built by the current builder instance.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// When overridden in a derived class, determines whether the builder
    /// instance can build the specified request.
    /// </summary>
    /// <param name="targetType">The type of the target request.</param>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    bool CanBuild(Type targetType);

    /// <summary>
    /// Builds the request for the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="context">The request context to act with.</param>
    void Build(RestRequestContext context);
}

/// <summary>
/// Builds the request for <see cref="IRestClient"/>.
/// </summary>
/// <typeparam name="TRestRequest">The type of the interface
/// implemented by the request source : <see cref="IRestContentBasicAuthentication"/>,
/// <see cref="IRestContentByteArray"/>, <see cref="IRestContentCookie"/>,
/// <see cref="IRestContentFormUrlEncoded"/>, <see cref="IRestContentHeader"/>,
/// <see cref="IRestContentMultipart"/>, <see cref="IRestContentPatch"/>,
/// <see cref="IRestContentPathString"/>, <see cref="IRestContentQueryString"/>,
/// <see cref="IRestContentStream"/> and <see cref="IRestContentString"/>.</typeparam>
public interface IRestRequestBuilder<TRestRequest> : IRestRequestBuilder
    where TRestRequest : class, IRestContent
{
    /// <summary>
    /// Gets the request type being built by the current builder instance.
    /// </summary>
    public new Type Type => typeof(TRestRequest);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type IRestRequestBuilder.Type => Type;
}
