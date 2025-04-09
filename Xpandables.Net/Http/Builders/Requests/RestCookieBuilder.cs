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
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Pipelines;

using static Xpandables.Net.Http.Rest;

namespace Xpandables.Net.Http.Builders.Requests;

/// <summary>
/// Builds cookies from the request context if the location is set to Cookie. 
/// It adds each cookie to the message options.
/// </summary>
public sealed class RestCookieBuilder<TRestRequest> :
    PipelineDecorator<RestRequestContext<TRestRequest>, ExecutionResult>, IRestRequestBuilder<TRestRequest>
    where TRestRequest : class, IRestCookie
{
    /// <inheritdoc/>
    protected override Task<ExecutionResult> HandleCoreAsync(
        RestRequestContext<TRestRequest> request,
        RequestHandler<ExecutionResult> next,
        CancellationToken cancellationToken = default)
    {
        if ((request.Attribute.Location & Location.Cookie) == Location.Cookie)
        {

            IDictionary<string, object?> cookieSource
                 = request.Request.GetCookieHeaderValue();

            foreach (KeyValuePair<string, object?> parameter in cookieSource)
            {
                _ = request.Message.Options
                    .TryAdd(parameter.Key, parameter.Value);
            }
        }

        return next();
    }
}
