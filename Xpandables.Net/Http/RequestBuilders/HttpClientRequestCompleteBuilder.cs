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
using System.Net.Http.Headers;

namespace Xpandables.Net.Http.RequestBuilders;
/// <summary>  
/// Represents a builder for completing HTTP client requests.  
/// </summary>  
public sealed class HttpClientRequestCompleteBuilder :
   HttpClientRequestBuilder<IRequestDefinitionComplete>
{
    /// <inheritdoc/>  
    public override int Order => int.MaxValue;

    /// <inheritdoc/>  
    public override bool CanBuild(Type targetType)
        => Type.IsAssignableFrom(targetType)
        || typeof(IHttpClientRequest).IsAssignableFrom(targetType);

    ///<inheritdoc/>  
    public override void Build(HttpClientRequestContext context)
    {
        if (context.Message.Content is not null)
        {
            context.Message.Content.Headers.ContentType
                = new MediaTypeHeaderValue(context.Attribute.ContentType);
        }

        if (context.Attribute.IsSecured)
        {
            context.Message.Options
                .Set(new(nameof(
                    HttpClientAttribute.IsSecured)),
                    context.Attribute.IsSecured);

            if (context.Message.Headers.Authorization is null)
            {
                context.Message.Headers.Authorization =
                    new AuthenticationHeaderValue(context.Attribute.Scheme);
            }
        }
    }
}
