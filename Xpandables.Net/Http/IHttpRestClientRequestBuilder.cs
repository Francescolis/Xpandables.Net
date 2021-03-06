﻿
/************************************************************************************************************
 * Copyright (C) 2020 Francis-Black EWANE
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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Xpandables.Net.Http
{
    /// <summary>
    /// Provides with <see cref="IHttpRestClientHandler"/> request builder.
    /// </summary>
    public interface IHttpRestClientRequestBuilder
    {
        /// <summary>
        /// The main method use to construct an <see cref="HttpRequestMessage"/> from the source .
        /// </summary>
        /// <typeparam name="TSource">The type of the object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="httpClient">The target HTTP client.</param>
        /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
        /// <returns>A task that represents an <see cref="HttpRequestMessage"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="source"/> must be decorated with
        /// <see cref="HttpRestClientAttribute"/> or implement <see cref="IHttpRestClientAttributeProvider"/>.</exception>
        Task<HttpRequestMessage> WriteHttpRequestMessageAsync<TSource>(
            TSource source,
            HttpClient httpClient,
            JsonSerializerOptions? serializerOptions = default)
            where TSource : class;

        /// <summary>
        /// Appends the given path keys and values to the Uri.
        /// </summary>
        /// <param name="path">The base Uri.</param>
        /// <param name="pathString">A collection of name value path pairs to append.</param>
        /// <returns>The combined result.</returns>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="pathString"/> is null.</exception>
        string AddPathString(string path, IDictionary<string, string> pathString);

        /// <summary>
        /// Appends the given query keys and values to the Uri.
        /// </summary>
        /// <param name="path">The base Uri.</param>
        /// <param name="queryString">A collection of name value query pairs to append.</param>
        /// <returns>The combined result.</returns>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> is null.</exception>
        string AddQueryString(string path, IDictionary<string, string?>? queryString);

        /// <summary>
        /// Writes location path using <see cref="AddPathString(string, IDictionary{string, string})"/> 
        /// and <see cref="IPathStringLocationRequest.GetPathStringSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <param name="attribute">The target attribute.</param>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        void WriteLocationPath<TSource>(TSource source, HttpRestClientAttribute attribute)
            where TSource : class;

        /// <summary>
        /// Writes location query using <see cref="AddQueryString(string, IDictionary{string, string?}?)"/> 
        /// and <see cref="IQueryStringLocationRequest.GetQueryStringSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <param name="attribute">The target attribute.</param>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        void WriteLocationQuery<TSource>(TSource source, HttpRestClientAttribute attribute)
            where TSource : class;

        /// <summary>
        /// Writes location cookies (add elements to <see cref="HttpRequestMessage.Options"/>) 
        /// using <see cref="ICookieLocationRequest.GetCookieSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <param name="attribute">The target attribute.</param>
        /// <param name="httpRequestMessage">The target request message.</param>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        void WriteLocationCookie<TSource>(
            TSource source,
            HttpRestClientAttribute attribute,
            HttpRequestMessage httpRequestMessage)
              where TSource : class;

        /// <summary>
        /// Writes location headers using <see cref="IHeaderLocationRequest.GetHeadersSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <param name="attribute">The target attribute.</param>
        /// <param name="httpRequestMessage">The target request message.</param>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        void WriteLocationHeader<TSource>(
            TSource source, 
            HttpRestClientAttribute attribute, 
            HttpRequestMessage httpRequestMessage)
            where TSource : class;

        /// <summary>
        /// Returns the source as byte array content using <see cref="IByteArrayRequest.GetByteContent"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <returns>A byte array content.</returns>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        [return: MaybeNull]
        HttpContent ReadByteArrayContent<TSource>(TSource source)
            where TSource : class;

        /// <summary>
        /// Returns the source as URL encoded content using <see cref="IFormUrlEncodedRequest.GetFormContent"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <returns>An URL encoded content.</returns>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        [return: MaybeNull]
        HttpContent ReadFormUrlEncodedContent<TSource>(TSource source)
            where TSource : class;

        /// <summary>
        /// Returns the source as string content using <see cref="IStringRequest.GetStringContent"/> 
        /// or <see cref="IPatchRequest"/> if available, if not use the hole source.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <param name="attribute">The target attribute.</param>
        /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
        /// <returns>A string content.</returns>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        HttpContent ReadStringContent<TSource>(
            TSource source,
            HttpRestClientAttribute attribute,
            JsonSerializerOptions? serializerOptions = default)
            where TSource : class;

        /// <summary>
        /// Returns the source as stream content using <see cref="IStreamRequest.GetStreamContent"/> 
        /// if available, if not use the hole source.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <param name="serializerOptions">Options to control the behavior during parsing.</param>
        /// <returns>A stream content.</returns>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        Task<HttpContent?> ReadStreamContentAsync<TSource>(
            TSource source,
            JsonSerializerOptions? serializerOptions = default)
          where TSource : class;

        /// <summary>
        /// Returns the source as multi part content using <see cref="IMultipartRequest"/> interface,
        /// using <see cref="ReadByteArrayContent{TSource}(TSource)"/> 
        /// and <see cref="ReadStringContent{TSource}(TSource, HttpRestClientAttribute, JsonSerializerOptions)"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object.</typeparam>
        /// <param name="source">The source object instance.</param>
        /// <param name="attribute">The target attribute.</param>
        /// <returns>A multi part content.</returns>
        /// <remarks>Used by <see cref="WriteHttpRequestMessageAsync{TSource}(TSource, HttpClient, JsonSerializerOptions?)"/></remarks>
        [return: MaybeNull]
        HttpContent ReadMultipartContent<TSource>(TSource source, HttpRestClientAttribute attribute)
            where TSource : class;
    }
}
