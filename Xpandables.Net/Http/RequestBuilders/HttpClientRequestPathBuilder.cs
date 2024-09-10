﻿
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using Xpandables.Net.Http.Requests;

using static Xpandables.Net.Http.Requests.HttpClientParameters;

namespace Xpandables.Net.Http.RequestBuilders;

/// <summary>
/// Build the path for a request.
/// </summary>
public sealed class HttpClientRequestPathBuilder :
    HttpClientRequestBuilder<IHttpRequestPathString>
{

    /// <inheritdoc/>
    public override int Order => 1;
    ///<inheritdoc/>
    public override void Build(HttpClientRequestContext context)
    {
        if ((context.Attribute.Location & Location.Path) != Location.Path)
        {
            return;
        }

        IHttpRequestPathString request = context
            .Request
            .AsRequired<IHttpRequestPathString>();

        IDictionary<string, string> pathString
            = request.GetPathStringSource();

        if (pathString.Count > 0)
        {
            context.RequestMessage.RequestUri =
                new Uri(AddPathString(
                    context.Attribute.Path
                    ?? context.RequestMessage
                        .RequestUri!
                        .AbsoluteUri, pathString),
                        UriKind.Relative);
        }
    }

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