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
/// Builds a request URI by adding path string parameters from the request context. It modifies the base path with
/// provided parameters.
/// </summary>
public sealed class RestPathStringBuilder<TRestRequest> :
    PipelineDecorator<RestRequestContext<TRestRequest>, ExecutionResult>, IRestRequestBuilder<TRestRequest>
    where TRestRequest : class, IRestPathString
{
    /// <inheritdoc/>
    protected override Task<ExecutionResult> HandleCoreAsync(
        RestRequestContext<TRestRequest> request,
        RequestHandler<ExecutionResult> next,
        CancellationToken cancellationToken = default)
    {
        if ((request.Attribute.Location & Location.Path) == Location.Path)
        {
            IDictionary<string, string> pathString = request.Request.GetPathString();

            if (pathString.Count > 0)
            {
                string path = AddPathString(request.Attribute.Path ?? request.Message.RequestUri!.AbsoluteUri, pathString);

                request.Message.RequestUri = new Uri(path, UriKind.Relative);
            }
        }

        return next();
    }

    /// <summary>
    /// Adds path string parameters to the given path.
    /// </summary>
    /// <param name="path">The base path.</param>
    /// <param name="pathString">The path string parameters to add.</param>
    /// <returns>The path with the added parameters.</returns>
    internal static string AddPathString(
        string path,
        IDictionary<string, string> pathString)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(pathString);

        if (pathString.Count == 0)
        {
            return path;
        }

        foreach (KeyValuePair<string, string> parameter in pathString)
        {
            path = path.Replace(
                $"{{{parameter.Key}}}",
                parameter.Value,
                StringComparison.InvariantCultureIgnoreCase);
        }

        return path;
    }
}
