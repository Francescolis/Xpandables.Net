
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
/// Builds the request content for HTTP requests based on the provided context. It handles stream content and multipart
/// form data.
/// </summary>
public sealed class RestStreamBuilder<TRestRequest> :
    PipelineDecorator<RestRequestContext<TRestRequest>, ExecutionResult>, IRestRequestBuilder<TRestRequest>
    where TRestRequest : class, IRestStream
{
    /// <inheritdoc/>
    protected override Task<ExecutionResult> HandleCoreAsync(
        RestRequestContext<TRestRequest> request,
        RequestHandler<ExecutionResult> next,
        CancellationToken cancellationToken = default)
    {
        if ((request.Attribute.Location & Location.Body) == Location.Body
            || request.Attribute.BodyFormat == BodyFormat.Stream)
        {
            StreamContent streamContent = request.Request.GetStreamContent();

            if (request.Message.Content is MultipartFormDataContent multiPartcontent)
            {
                if (request.Request is IRestMultipart)
                {
                    multiPartcontent.Add(streamContent);
                }
                else
                {
                    multiPartcontent.Add(streamContent);
                    request.Message.Content = multiPartcontent;
                }
            }
            else
            {
                request.Message.Content = streamContent;
            }
        }

        return next();
    }
}
