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
using System.Text;
using System.Text.Json;

using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Pipelines;

using static Xpandables.Net.Http.Rest;

namespace Xpandables.Net.Http.Builders.Requests;

/// <summary>
/// Builds the request content for a REST API call based on the provided context. It serializes string content and adds
/// it to the request message.
/// </summary>
public sealed class RestStringBuilder<TRestRequest> :
    PipelineDecorator<RestRequestContext<TRestRequest>, ExecutionResult>, IRestRequestBuilder<TRestRequest>
    where TRestRequest : class, IRestString
{
    /// <inheritdoc/>
    protected override Task<ExecutionResult> HandleCoreAsync(
        RestRequestContext<TRestRequest> request,
        RequestHandler<ExecutionResult> next,
        CancellationToken cancellationToken = default)
    {
        if ((request.Attribute.Location & Location.Body) == Location.Body
            || request.Attribute.BodyFormat == BodyFormat.String)
        {

#pragma warning disable CA2000 // Dispose objects before losing scope
            StringContent content = new(
                JsonSerializer.Serialize(
                    request.Request.GetStringContent(),
                    request.SerializerOptions),
                Encoding.UTF8,
                request.Attribute.ContentType);
#pragma warning restore CA2000 // Dispose objects before losing scope

            if (request.Message.Content is MultipartFormDataContent multipart)
            {
                multipart.Add(content);
            }
            else
            {
                request.Message.Content = content;
            }
        }

        return next();
    }
}
