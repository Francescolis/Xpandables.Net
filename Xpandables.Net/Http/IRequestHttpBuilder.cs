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

namespace Xpandables.Net.Http;

/// <summary>
/// Builds the request for <see cref="IRequestHttpSender"/>.
/// </summary>
public interface IRequestHttpBuilder
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
    void Build(RequestContext context);
}

/// <summary>
/// Builds the request for <see cref="IRequestHttpSender"/>.
/// </summary>
/// <typeparam name="TRequestDefinition">The type of the interface
/// implemented by the request source : <see cref="IRequestBasicAuthentication"/>,
/// <see cref="IRequestByteArray"/>, <see cref="IRequestCookie"/>,
/// <see cref="IRequestFormUrlEncoded"/>, <see cref="IRequestHeader"/>,
/// <see cref="IRequestMultipart"/>, <see cref="IRequestPatch"/>,
/// <see cref="IRequestPathString"/>, <see cref="IRequestQueryString"/>,
/// <see cref="IRequestStream"/> and <see cref="IRequestString"/>.</typeparam>
public interface IRequestHttpBuilder<TRequestDefinition> :
    IRequestHttpBuilder
    where TRequestDefinition : class, IRequestHttpDefinition
{
    /// <summary>
    /// Gets the request type being built by the current builder instance.
    /// </summary>
    public new Type Type => typeof(TRequestDefinition);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Type IRequestHttpBuilder.Type => Type;
}
