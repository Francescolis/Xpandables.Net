
/************************************************************************************************************
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
************************************************************************************************************/
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides with methods to build <see cref="HttpRequestMessage"/> for use with <see cref="IHttpClientDispatcher"/>.
/// </summary>
public interface IHttpClientRequestBuilder
{
    /// <summary>
    /// The method used to construct an <see cref="HttpRequestMessage"/> from the source.
    /// The <paramref name="source"/> may implement some interfaces such as <see cref="IHttpRequestHeader"/>, <see cref="IHttpRequestString"/> and so on.
    /// </summary>
    /// <typeparam name="TSource">The type of the object.</typeparam>
    /// <param name="source">The source object to act on.</param>
    /// <param name="httpClient">The target HTTP client.</param>
    /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
    /// <returns>A task that represents an <see cref="HttpRequestMessage"/> object.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is null.</exception>
    /// <exception cref="ArgumentException">The <paramref name="source"/> must be decorated with
    /// <see cref="HttpClientAttribute"/> or implement <see cref="IHttpClientAttributeBuilder"/>.</exception>
    /// <exception cref="InvalidOperationException">Unable to build the request message.</exception>
    ValueTask<HttpRequestMessage> BuildHttpRequestAsync<TSource>(
        TSource source,
        HttpClient httpClient,
        JsonSerializerOptions? serializerOptions = default)
           where TSource : class;
}

internal sealed class HttpClientRequestBuilderInternal(IServiceProvider serviceProvider) : IHttpClientRequestBuilder
{
    public ValueTask<HttpRequestMessage> BuildHttpRequestAsync<TSource>(
        TSource source,
        HttpClient httpClient,
        JsonSerializerOptions? serializerOptions = null)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);

        var attribute = ReadHttpRestClientAttribute(source, serviceProvider);

        attribute.Path ??= "/";

        if ((attribute.Location & Location.Path) == Location.Path
            || (attribute.Location & Location.Query) == Location.Query)
        {
            WriteLocationPath(source, attribute);
            WriteLocationQuery(source, attribute);
        }

        attribute.Uri = new Uri(attribute.Path, UriKind.Relative);
#pragma warning disable CA2000 // Dispose objects before losing scope
        var httpRequestMessage = new HttpRequestMessage(new HttpMethod(attribute.Method.ToString()), attribute.Uri);
#pragma warning restore CA2000 // Dispose objects before losing scope
        httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(attribute.Accept));
        httpRequestMessage.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(Thread.CurrentThread.CurrentCulture.Name));

        WriteLocationCookie(source, attribute, httpRequestMessage);
        WriteLocationHeader(source, attribute, httpRequestMessage);

        if (!attribute.IsNullable && (attribute.Location & Location.Body) == Location.Body)
        {
            httpRequestMessage.Content = attribute.BodyFormat switch
            {
                BodyFormat.ByteArray => ReadByteArrayContent(source),
                BodyFormat.FormUrlEncoded => ReadFormUrlEncodedContent(source),
                BodyFormat.Multipart => ReadMultipartContent(source, attribute),
                BodyFormat.Stream => ReadStreamContent(source),
                _ => ReadStringContent(source, attribute, serializerOptions)
            };

            if (httpRequestMessage.Content is not null && httpRequestMessage.Content.Headers.ContentType is null)
                httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(attribute.ContentType);
        }

        if (attribute.IsSecured)
        {
            httpRequestMessage.Headers.Authorization =
                httpClient.DefaultRequestHeaders.Authorization ?? new AuthenticationHeaderValue(attribute.Scheme);
            httpRequestMessage.Options.Set(new(nameof(HttpClientAttribute.IsSecured)), attribute.IsSecured);
        }

        return ValueTask.FromResult(httpRequestMessage);

        static HttpClientAttribute ReadHttpRestClientAttribute(TSource source, IServiceProvider serviceProvider)
        {
            if (source is IHttpClientAttributeBuilder httpRestClientAttributeProvider)
                return httpRestClientAttributeProvider.Build(serviceProvider);

            return source.GetType().GetCustomAttribute<HttpClientAttribute>()
                ?? throw new ArgumentNullException(
                    $"{source.GetType().Name} must be decorated with {nameof(HttpClientAttribute)} " +
                    $"attribute or implement {nameof(IHttpClientAttributeBuilder)} interface.");
        }
    }

    internal static string AddPathString(string path, IDictionary<string, string> pathString)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(pathString);

        if (pathString.Count == 0)
            return path;

        foreach (var parameter in pathString)
            path = path.Replace(
                $"{{{parameter.Key}}}",
                parameter.Value,
                StringComparison.InvariantCultureIgnoreCase);

        return path;
    }

    internal static void WriteLocationPath<TSource>(TSource source, HttpClientAttribute attribute)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if ((attribute.Location & Location.Path) != Location.Path)
            return;

        ValidateInterfaceImplementation<IHttpRequestPathString>(source);
        if (source is not IHttpRequestPathString pathStringRequest)
            return;

        var pathString = pathStringRequest.GetPathStringSource();
        attribute.Path = AddPathString(attribute.Path!, pathString);
    }

    internal static void WriteLocationQuery<TSource>(TSource source, HttpClientAttribute attribute)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if ((attribute.Location & Location.Query) != Location.Query)
            return;

        ValidateInterfaceImplementation<IHttpRequestQueryString>(source);
        if (source is not IHttpRequestQueryString queryStringRequest)
            return;

        var queryString = queryStringRequest.GetQueryStringSource();
        attribute.Path = attribute.Path!.AddQueryString(queryString);
    }

    internal static void WriteLocationCookie<TSource>(
        TSource source, HttpClientAttribute attribute, HttpRequestMessage httpRequestMessage)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentNullException.ThrowIfNull(httpRequestMessage);

        if ((attribute.Location & Location.Cookie) != Location.Cookie)
            return;

        ValidateInterfaceImplementation<IHttpRequestCookie>(source);
        if (source is not IHttpRequestCookie cookieLocationRequest)
            return;

        var cookieSource = cookieLocationRequest.GetCookieSource();
        foreach (var parameter in cookieSource)
            httpRequestMessage.Options.TryAdd(parameter.Key, parameter.Value);
    }

    internal static void WriteLocationHeader<TSource>(
        TSource source, HttpClientAttribute attribute, HttpRequestMessage httpRequestMessage)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentNullException.ThrowIfNull(httpRequestMessage);

        if ((attribute.Location & Location.Header) != Location.Header) return;

        ValidateInterfaceImplementation<IHttpRequestHeader>(source);
        if (source is not IHttpRequestHeader headerLocationRequest) return;

        var headerSource = headerLocationRequest.GetHeadersSource();
        if (headerLocationRequest.GetHeaderModelName() is string modelName)
        {
            string headerValue = string.Join(",", headerSource.Select(x => $"{x.Key},{string.Join(";", x.Value)}"));
            httpRequestMessage.Headers.Add(modelName, headerValue);
        }
        else
        {
            foreach (var parameter in headerSource)
            {
                httpRequestMessage.Headers.Remove(parameter.Key);
                httpRequestMessage.Headers.Add(parameter.Key, parameter.Value);
            }
        }
    }

    internal static HttpContent? ReadByteArrayContent<TSource>(TSource source) where TSource : class
    {
        ValidateInterfaceImplementation<IHttpRequestByteArray>(source);
        if (source is IHttpRequestByteArray byteArrayRequest
            && byteArrayRequest.GetByteContent() is { } byteContent)
        {
            return byteContent;
        }

        return default;
    }

    internal static HttpContent? ReadFormUrlEncodedContent<TSource>(TSource source) where TSource : class
    {
        ValidateInterfaceImplementation<IHttpRequestFormUrlEncoded>(source);
        if (source is IHttpRequestFormUrlEncoded formUrlEncodedRequest
            && formUrlEncodedRequest.GetFormSource() is { } formContent)
        {
            return new FormUrlEncodedContent(formContent);
        }

        return default;
    }

    internal static StringContent? ReadStringContent<TSource>(
        TSource source, HttpClientAttribute attribute, JsonSerializerOptions? serializerOptions = default)
        where TSource : class
    {
        ValidateInterfaceImplementation<IHttpRequestString>(source, true);
        if (source is IHttpRequestString stringRequest)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            return new StringContent(
                JsonSerializer.Serialize(stringRequest.GetStringContent(), serializerOptions),
                Encoding.UTF8,
                attribute.ContentType);
        }

        ValidateInterfaceImplementation<IHttpRequestPatch>(source, true);
        if (source is IHttpRequestPatch patchRequest)
        {
            return new StringContent(
                JsonSerializer.Serialize(patchRequest.PatchOperations, serializerOptions),
                Encoding.UTF8,
                attribute.ContentType);
        }

        return default;
    }

    internal static StreamContent? ReadStreamContent<TSource>(TSource source)
        where TSource : class
    {
        ValidateInterfaceImplementation<IHttpRequestStream>(source);
        if (source is not IHttpRequestStream streamRequest)
            return default;

        if (streamRequest.GetStreamContent() is not { } streamContent)
            return default;

        if (source is IHttpRequestMultipart multipartRequest && streamContent.Headers.ContentType is null)
            if (new HttpClientMime().GetMimeType(multipartRequest.GetFileName()) is string mediaType)
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

        return streamContent;
    }

    internal static HttpContent? ReadMultipartContent<TSource>(TSource source, HttpClientAttribute attribute)
        where TSource : class
    {
        ValidateInterfaceImplementation<IHttpRequestMultipart>(source);
        if (source is not IHttpRequestMultipart multipartRequest) return default;

        var multipartContent = new MultipartFormDataContent();
        if (ReadStreamContent(multipartRequest) is { } streamContent)
            multipartContent.Add(streamContent, multipartRequest.GetName(), multipartRequest.GetFileName());

#pragma warning disable CA2000 // Dispose objects before losing scope
        if (ReadStringContent(multipartRequest, attribute) is { } stringContent)
            multipartContent.Add(stringContent);
#pragma warning restore CA2000 // Dispose objects before losing scope

        return multipartContent;
    }

    internal static void ValidateInterfaceImplementation<TInterface>(object source, bool implementationIsOptional = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is not TInterface && !implementationIsOptional)
        {
            throw new ArgumentException($"{source.GetType().Name} must implement {typeof(TInterface).Name} interface");
        }
    }
}
