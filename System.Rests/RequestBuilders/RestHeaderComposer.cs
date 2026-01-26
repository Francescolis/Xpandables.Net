/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Collections;
using System.Rests.Abstractions;

using static System.Rests.Abstractions.RestSettings;

namespace System.Rests.RequestBuilders;

/// <summary>
/// Composes HTTP request headers from a given RestRequestContext. It adds headers based on the request's model name or
/// directly from the header source.
/// </summary>
public sealed class RestHeaderComposer : IRestRequestComposer
{
    /// <inheritdoc/>
    public bool CanCompose(RestRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Request is IRestHeader
            && context.Attribute.Location.HasFlag(Location.Header);
    }

    /// <inheritdoc/>
    public ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (!CanCompose(context))
        {
            throw new InvalidOperationException("The current composer cannot compose the given request context.");
        }

        ElementCollection headerSource = ((IRestHeader)context.Request).GetHeaders();

        if (((IRestHeader)context.Request).GetHeaderModelName() is string modelName)
        {
            string headerValue = string.Join(
                ';',
                headerSource.Select(entry => $"{entry.Key},{string.Join(',', entry.Values.ToString())}"));

            context.Message
                .Headers
                .Add(modelName, headerValue);
        }
        else
        {
            foreach (ElementEntry parameter in headerSource)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _ = context.Message
                        .Headers
                        .Remove(parameter.Key);

                context.Message
                    .Headers
                    .Add(parameter.Key, values: parameter.Values);
            }
        }
        return ValueTask.CompletedTask;
    }
}