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
using System.Diagnostics.CodeAnalysis;
using System.Rests.Abstractions;

using static System.Rests.Abstractions.RestSettings;

namespace System.Rests.RequestBuilders;

/// <summary>
/// Composes a request URI by adding path string parameters from the request context. It modifies the base path with
/// provided parameters.
/// </summary>
public sealed class RestPathStringComposer<TRestRequest> : IRestRequestComposer<TRestRequest>
    where TRestRequest : class, IRestPathString
{
    /// <inheritdoc/>
    public ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if ((context.Attribute.Location & Location.Path) != Location.Path)
        {
            return ValueTask.CompletedTask;
        }

        IDictionary<string, string> pathString = ((IRestPathString)context.Request).GetPathString();

        if (pathString.Count > 0)
        {
            string basePath = context.Attribute.Path
                ?? context.Message.RequestUri?.OriginalString
                ?? "/";

            string path = AddPathString(basePath, pathString);

            context.Message.RequestUri = new Uri(path, UriKind.RelativeOrAbsolute);
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Adds path string parameters to the given path.
    /// </summary>
    /// <param name="path">The base path.</param>
    /// <param name="pathString">The path string parameters to add.</param>
    /// <returns>The path with the added parameters.</returns>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
    public static string AddPathString(
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
