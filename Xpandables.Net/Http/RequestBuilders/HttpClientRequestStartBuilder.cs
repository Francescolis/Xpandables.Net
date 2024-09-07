
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
using System.Net.Http.Headers;

using Xpandables.Net.Http.Requests;

namespace Xpandables.Net.Http.RequestBuilders;

/// <summary>
/// Starts the base <see cref="HttpRequestMessage"/> for a request.
/// </summary>
public sealed class HttpClientRequestStartBuilder :
    HttpClientRequestBuilder<IHttpRequest>
{
    /// <inheritdoc/>
    public override int Order => int.MinValue;
    /// <inheritdoc/>
    public override bool CanBuild(Type targetType)
        => Type.IsAssignableFrom(targetType)
        || typeof(IHttpClientRequest).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override void Build(HttpClientRequestContext context)
    {
        context.Attribute.Path ??= "/";

        context.RequestMessage.Method = new(context.Attribute.Method.ToString());
        context.RequestMessage.RequestUri = new(context.Attribute.Path, UriKind.Relative);

        context.RequestMessage.Headers.Accept
            .Add(new MediaTypeWithQualityHeaderValue(context.Attribute.Accept));
        context.RequestMessage.Headers.AcceptLanguage
            .Add(new StringWithQualityHeaderValue(
                Thread.CurrentThread.CurrentCulture.Name));
    }
}
