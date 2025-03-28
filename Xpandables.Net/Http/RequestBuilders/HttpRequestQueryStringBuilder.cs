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
using System.Text;
using System.Text.Encodings.Web;

using static Xpandables.Net.Http.MapRequest;

namespace Xpandables.Net.Http.RequestBuilders;

/// <summary>
/// Provides a builder for constructing HTTP client request query strings.
/// </summary>
public sealed class HttpRequestQueryStringBuilder : HttpRequestBuilder<IHttpRequestContentQueryString>
{
    ///<inheritdoc/>
    public override void Build(RequestContext context)
    {
        if ((context.Attribute.Location & Location.Query) != Location.Query)
        {
            return;
        }

        IHttpRequestContentQueryString request = (IHttpRequestContentQueryString)context.Request;

        IDictionary<string, string?>? queryString = request.GetQueryString();

        string path = context.Attribute.Path
            ?? context.Message.RequestUri!.AbsoluteUri;

        string queryStringPath = path.AddQueryString(queryString);

        context.Message.RequestUri = new Uri(queryStringPath, UriKind.RelativeOrAbsolute);
    }
}

/// <summary>
/// Provides extension methods for adding query strings to URIs.
/// </summary>
public static class HttpClientRequestQueryStringExtensions
{
    /// <summary>
    /// Adds the specified query string to the given path.
    /// </summary>
    /// <param name="path">The base path to which the query string will be added.</param>
    /// <param name="queryString">The query string parameters to add.</param>
    /// <returns>The path with the query string appended.</returns>
    public static string AddQueryString(
    this string path,
    IDictionary<string, string?>? queryString)
    {
        // From MS internal code
        ArgumentNullException.ThrowIfNull(path);

        if (queryString is null)
        {
            return path;
        }

        int anchorIndex = path.IndexOf('#', StringComparison.InvariantCulture);
        string uriToBeAppended = path;
        string anchorText = "";

        // If there is an anchor, then the request string must be inserted
        // before its first occurrence.
        if (anchorIndex != -1)
        {
            anchorText = path[anchorIndex..];
            uriToBeAppended = path[..anchorIndex];
        }

        bool hasQuery = uriToBeAppended.Contains('?', StringComparison.InvariantCulture);

        StringBuilder sb = new();
        _ = sb.Append(uriToBeAppended);
        foreach (KeyValuePair<string, string?> parameter in queryString)
        {
            _ = sb.Append(hasQuery ? '&' : '?');
            _ = sb.Append(UrlEncoder.Default.Encode(parameter.Key));
            _ = sb.Append('=');
            _ = sb.Append(parameter.Value is null
                ? null
                : UrlEncoder.Default.Encode(parameter.Value));
            hasQuery = true;
        }

        _ = sb.Append(anchorText);
        return sb.ToString();
    }
}
