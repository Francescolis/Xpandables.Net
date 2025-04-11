﻿/*******************************************************************************
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
using Xpandables.Net.Collections;

using static Xpandables.Net.Executions.Rests.Rest;

namespace Xpandables.Net.Executions.Rests.Requests;

/// <summary>
/// Composes HTTP request headers from a given RestRequestContext. It adds headers based on the request's model name or
/// directly from the header source.
/// </summary>
public sealed class RestHeaderComposer<TRestRequest> : IRestRequestComposer<TRestRequest>
    where TRestRequest : class, IRestHeader
{
    /// <inheritdoc/>
    public void Compose(RestRequestContext<TRestRequest> context)
    {
        if ((context.Attribute.Location & Location.Header) == Location.Header)
        {
            ElementCollection headerSource = context.Request.GetHeaders();

            if (context.Request.GetHeaderModelName() is string modelName)
            {
                string headerValue = string.Join(
                    ";",
                    headerSource
                        .Select(x => $"{x.Key},{x.Values.StringJoin(",")}"));

                context.Message
                    .Headers
                    .Add(modelName, headerValue);
            }
            else
            {
                foreach (ElementEntry parameter in headerSource)
                {
                    _ = context.Message
                            .Headers
                            .Remove(parameter.Key);

                    context.Message
                        .Headers
                        .Add(parameter.Key, values: parameter.Values);
                }
            }
        }
    }
}