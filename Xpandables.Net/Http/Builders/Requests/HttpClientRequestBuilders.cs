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

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Xpandables.Net.Http.Builders.Requests;

/// <summary>
/// Build the attribute for a request.
/// </summary>
public sealed class HttpClientAttributeBuilder :
    HttpClientRequestAttributeBuilder
{
    ///<inheritdoc/>
    public override Type? Type => typeof(IHttpClientAttributeProvider);

    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpClientAttributeProvider).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpClientAttribute Build(
        IHttpClientAttributeProvider request,
        IServiceProvider serviceProvider)
        => request.Build(serviceProvider);
}

/// <summary>
/// Build the path for a request.
/// </summary>
public sealed class HttpClientRequestPathBuilder :
    HttpClientRequestBuilder<IHttpRequestPathString>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestPathString).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestPathString request,
        HttpRequestMessage requestMessage)
    {
        IDictionary<string, string> pathString
           = request.GetPathStringSource();

        if (pathString.Count > 0)
            requestMessage.RequestUri =
                new Uri(AddPathString(
                    attribute.Path ?? requestMessage.RequestUri!.AbsoluteUri, pathString), UriKind.Relative);

        return requestMessage;
    }

    internal static string AddPathString(
        string path,
        IDictionary<string, string> pathString)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(pathString);

        if (pathString.Count == 0)
            return path;

        foreach (KeyValuePair<string, string> parameter in pathString)
            path = path.Replace(
                $"{{{parameter.Key}}}",
                parameter.Value,
                StringComparison.InvariantCultureIgnoreCase);

        return path;
    }
}

/// <summary>
/// Build the query string for a request.
/// </summary>
public sealed class HttpClientRequestQueryStringBuilder :
    HttpClientRequestBuilder<IHttpRequestQueryString>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestQueryString).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestQueryString request,
        HttpRequestMessage requestMessage)
    {
        IDictionary<string, string?>? queryString
           = request.GetQueryStringSource();

        requestMessage.RequestUri =
            new Uri((attribute.Path ?? requestMessage.RequestUri!.AbsoluteUri)
            .AddQueryString(queryString), UriKind.Relative);

        return requestMessage;
    }
}

/// <summary>
/// Build the cookie for a request.
/// </summary>
public sealed class HttpClientRequestCookieBuilder :
    HttpClientRequestBuilder<IHttpRequestCookie>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestCookie).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestCookie request,
        HttpRequestMessage requestMessage)
    {
        IDictionary<string, object?> cookieSource
             = request.GetCookieSource();

        foreach (KeyValuePair<string, object?> parameter in cookieSource)
            _ = requestMessage.Options
                .TryAdd(parameter.Key, parameter.Value);

        return requestMessage;
    }
}

/// <summary>
/// Build the header for a request.
/// </summary>
public sealed class HttpClientRequestHeaderBuilder :
    HttpClientRequestBuilder<IHttpRequestHeader>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestHeader).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestHeader request,
        HttpRequestMessage requestMessage)
    {
        IDictionary<string, IEnumerable<string?>> headerSource
            = request.GetHeadersSource();

        if (request.GetHeaderModelName() is string modelName)
        {
            string headerValue = string.Join(
                ",",
                headerSource
                    .Select(x => $"{x.Key},{string.Join(";", x.Value)}"));

            requestMessage
                .Headers
                .Add(modelName, headerValue);
        }
        else
        {
            foreach (KeyValuePair<string, IEnumerable<string?>> parameter
                in headerSource)
            {
                _ = requestMessage
                        .Headers
                        .Remove(parameter.Key);

                requestMessage
                    .Headers
                    .Add(parameter.Key, parameter.Value);
            }
        }

        return requestMessage;
    }
}

/// <summary>
/// Build the byte content for a request.
/// </summary>
public sealed class HttpClientRequestByteArrayBuilder :
    HttpClientRequestBuilder<IHttpRequestByteArray>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestByteArray).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestByteArray request,
        HttpRequestMessage requestMessage)
    {
        if (request.GetByteContent() is { } byteArray)
            requestMessage.Content = byteArray;

        return requestMessage;
    }
}

/// <summary>
/// Build the form url encoded content for a request.
/// </summary>
public sealed class HttpClientRequestFormUrlEncodedBuilder :
    HttpClientRequestBuilder<IHttpRequestFormUrlEncoded>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestFormUrlEncoded).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestFormUrlEncoded request,
        HttpRequestMessage requestMessage)
    {
        if (request.GetFormSource() is { } formContent)
            requestMessage.Content = new FormUrlEncodedContent(formContent);

        return requestMessage;
    }
}

/// <summary>
/// Build the form url encoded content for a request.
/// </summary>
public sealed class HttpClientRequestMultipartBuilder :
    HttpClientRequestBuilder<IHttpRequestMultipart>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestMultipart).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestMultipart request,
        HttpRequestMessage requestMessage)
    {
        MultipartFormDataContent multipartContent = [];
        requestMessage.Content = multipartContent;

        return requestMessage;
    }
}

/// <summary>
/// Build the stream content for a request.
/// </summary>
public sealed class HttpClientRequestStreamBuilder :
    HttpClientRequestBuilder<IHttpRequestStream>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestStream).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestStream request,
        HttpRequestMessage requestMessage)
    {
        if (request.GetStreamContent() is { } streamContent)
        {
            if (request is IHttpRequestMultipart multipartRequest
                && streamContent.Headers.ContentType is null)
                if (new HttpClientMime().GetMimeType
                    (multipartRequest.GetFileName()) is string mediaType)
                    streamContent.Headers.ContentType
                        = new MediaTypeHeaderValue(mediaType);

            if (requestMessage.Content is MultipartFormDataContent content)
                if (request is IHttpRequestMultipart multipart)
                {
                    content.Add(
                        streamContent,
                        multipart.GetName(),
                        multipart.GetFileName());
                }
                else
                {
                    content.Add(streamContent);
                    requestMessage.Content = content;
                }

            else
                requestMessage.Content = streamContent;
        }

        return requestMessage;
    }
}

/// <summary>
/// Build the string content for a request.
/// </summary>
public sealed class HttpClientRequestStringBuilder :
    HttpClientRequestBuilder<IHttpRequestString>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestString).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestString request,
        HttpRequestMessage requestMessage)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        StringContent content = new(
            JsonSerializer.Serialize(
                request.GetStringContent(),
                SerializerOptions),
            Encoding.UTF8,
            attribute.ContentType);
#pragma warning restore CA2000 // Dispose objects before losing scope

        if (requestMessage.Content is MultipartFormDataContent multipart)
            multipart.Add(content);
        else
            requestMessage.Content = content;

        return requestMessage;
    }
}

/// <summary>
/// Build the Patch content for a request.
/// </summary>
public sealed class HttpClientRequestPatchBuilder :
    HttpClientRequestBuilder<IHttpRequestPatch>
{
    ///<inheritdoc/>
    public override bool CanBuild(Type targetType)
        => typeof(IHttpRequestPatch).IsAssignableFrom(targetType);

    ///<inheritdoc/>
    public override HttpRequestMessage Build(
        HttpClientAttribute attribute,
        IHttpRequestPatch request,
        HttpRequestMessage requestMessage)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        StringContent content = new(
            JsonSerializer.Serialize(
                request.PatchOperations,
                SerializerOptions),
            Encoding.UTF8,
            attribute.ContentType);
#pragma warning restore CA2000 // Dispose objects before losing scope

        if (requestMessage.Content is MultipartFormDataContent multipart)
            multipart.Add(content);
        else
            requestMessage.Content = content;

        return requestMessage;
    }
}