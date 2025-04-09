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
using Xpandables.Net.Collections;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Pipelines;

using static Xpandables.Net.Http.Rest;

namespace Xpandables.Net.Http.Builders.Requests;

/// <summary>
/// Builds HTTP request headers from a given RestRequestContext. It adds headers based on the request's model name or
/// directly from the header source.
/// </summary>
public sealed class RestHeaderBuilder<TRestRequest> :
    PipelineDecorator<RestRequestContext<TRestRequest>, ExecutionResult>, IRestRequestBuilder<TRestRequest>
    where TRestRequest : class, IRestHeader
{
    /// <inheritdoc/>
    protected override Task<ExecutionResult> HandleCoreAsync(
        RestRequestContext<TRestRequest> request,
        RequestHandler<ExecutionResult> next,
        CancellationToken cancellationToken = default)
    {
        if ((request.Attribute.Location & Location.Header) == Location.Header)
        {
            ElementCollection headerSource = request.Request.GetHeaders();

            if (request.Request.GetHeaderModelName() is string modelName)
            {
                string headerValue = string.Join(
                    ";",
                    headerSource
                        .Select(x => $"{x.Key},{x.Values.StringJoin(",")}"));

                request.Message
                    .Headers
                    .Add(modelName, headerValue);
            }
            else
            {
                foreach (ElementEntry parameter in headerSource)
                {
                    _ = request.Message
                            .Headers
                            .Remove(parameter.Key);

                    request.Message
                        .Headers
                        .Add(parameter.Key, values: parameter.Values);
                }
            }
        }

        return next();
    }
}