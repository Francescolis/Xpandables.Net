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

using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Pipelines;

namespace Xpandables.Net.Http.Builders.Requests;

/// <summary>
/// Builds and configures a REST request context, setting various attributes and headers based on the provided request
/// context.
/// </summary>
/// <typeparam name="TRestRequest">Represents a specific type of request context that adheres to the required interface for processing.</typeparam>
public sealed class RestContextBuilder<TRestRequest> :
    PipelineDecorator<RestRequestContext<TRestRequest>, ExecutionResult>, IRestRequestBuilder<TRestRequest>
    where TRestRequest : class, IRestContext
{
    /// <inheritdoc/>
    protected override async Task<ExecutionResult> HandleCoreAsync(
        RestRequestContext<TRestRequest> request,
        RequestHandler<ExecutionResult> next,
        CancellationToken cancellationToken = default)
    {
        request.Attribute.Path ??= "/";
        request.Message.Method = new(request.Attribute.Method.ToString());
        request.Message.RequestUri = new(request.Attribute.Path, UriKind.Relative);

        request.Message.Headers.Accept
            .Add(new MediaTypeWithQualityHeaderValue(request.Attribute.Accept));
        request.Message.Headers.AcceptLanguage
            .Add(new StringWithQualityHeaderValue(
                Thread.CurrentThread.CurrentCulture.Name));

        cancellationToken.ThrowIfCancellationRequested();

        _ = await next().ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (request.Message.Content is not null)
        {
            request.Message.Content.Headers.ContentType
                = new MediaTypeHeaderValue(request.Attribute.ContentType);
        }

        if (request.Attribute.IsSecured)
        {
            request.Message.Options
                .Set(new(nameof(
                    RestAttribute.IsSecured)),
                    request.Attribute.IsSecured);

            if (request.Message.Headers.Authorization is null)
            {
                request.Message.Headers.Authorization =
                    new AuthenticationHeaderValue(request.Attribute.Scheme);
            }
        }

        return ExecutionResults.Success();
    }
}
