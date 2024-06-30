
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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides with methods to extend <see cref="HttpClientResponse"/>.
/// </summary>
public static class HttpClientDispatcherExtensions
{
    /// <summary>
    /// Returns the headers found in the specified response.
    /// </summary>
    /// <param name="httpResponse">The response to act on.</param>
    /// <returns>An instance of <see cref="NameValueCollection"/>.</returns>
    public static NameValueCollection ReadHttpResponseHeaders(
        this HttpResponseMessage httpResponse)
    {
        ArgumentNullException.ThrowIfNull(httpResponse);

        return Enumerable
                .Empty<(string Name, string Value)>()
                .Concat(
                    httpResponse.Headers
                        .SelectMany(kvp => kvp.Value
                            .Select(v => (Name: kvp.Key, Value: v))
                            )
                        )
                .Concat(
                    httpResponse.Content.Headers
                        .SelectMany(kvp => kvp.Value
                            .Select(v => (Name: kvp.Key, Value: v))
                        )
                        )
                .Aggregate(
                    seed: new NameValueCollection(),
                    func: (nvc, pair) =>
                    {
                        (string name, string value) = pair;
                        nvc.Add(name, value); return nvc;
                    },
                    resultSelector: nvc => nvc
                    );
    }

    /// <summary>
    /// Returns the headers from the instance.
    /// </summary>
    /// <param name="headers">The collection of headers to act on.</param>
    /// <returns>An instance of <see cref="NameValueCollection"/>.</returns>
    public static NameValueCollection ReadHttpHeaders(this HttpHeaders headers)
        => Enumerable
            .Empty<(string Name, string Value)>()
            .Concat(
                headers
                    .SelectMany(kvp => kvp.Value
                        .Select(v => (Name: kvp.Key, Value: v))
                        )
                    )
            .Aggregate(
                seed: new NameValueCollection(),
                func: (nvc, pair) =>
                {
                    (string name, string value) = pair;
                    nvc.Add(name, value); return nvc;
                },
                resultSelector: nvc => nvc
                );

    /// <summary>
    /// Sends the request that returns a collection that can be async-enumerated.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="httpRestClientHandler">the current handler instance.</param>
    /// <param name="request">The request to act with. T
    /// he request must be decorated with 
    /// the <see cref="HttpClientAttribute"/> or implements 
    /// the <see cref="IHttpClientAttributeProvider"/> interface.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>Returns a task 
    /// <see cref="HttpClientResponse{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    public static Task<HttpClientResponse<IAsyncEnumerable<TResult>>>
        SendAsync<TResult>(
        this IHttpClientDispatcher httpRestClientHandler,
        IHttpClientAsyncRequest<TResult> request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpRestClientHandler);
        return httpRestClientHandler
            .SendAsync(request, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends the request that returns a collection that can be async-enumerated.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="httpRestClientHandler">the current handler instance.</param>
    /// <param name="request">The request to act with. The request must be decorated with 
    /// the <see cref="HttpClientAttribute"/> or implements the 
    /// <see cref="IHttpClientAttributeProvider"/> interface.</param>
    /// <param name="serializerOptions">Options to control 
    /// the behavior during parsing.</param>
    /// <returns>Returns a task <see cref="HttpClientResponse{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    public static Task<HttpClientResponse<IAsyncEnumerable<TResult>>>
        SendAsync<TResult>(
        this IHttpClientDispatcher httpRestClientHandler,
        IHttpClientAsyncRequest<TResult> request,
        JsonSerializerOptions serializerOptions)
    {
        ArgumentNullException.ThrowIfNull(httpRestClientHandler);
        return httpRestClientHandler
            .SendAsync(request, serializerOptions: serializerOptions);
    }

    /// <summary>
    /// Sends the request that does not return a response.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <param name="httpRestClientHandler">the current 
    /// handler instance.</param>
    /// <param name="request">The request to act with. 
    /// The request must be decorated with 
    /// the <see cref="HttpClientAttribute"/> or implements 
    /// the <see cref="IHttpClientAttributeProvider"/> interface.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <returns>Returns a task <see cref="HttpClientResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    public static Task<HttpClientResponse> SendAsync(
        this IHttpClientDispatcher httpRestClientHandler,
        IHttpClientRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpRestClientHandler);
        return httpRestClientHandler
            .SendAsync(request, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends the request that does not return a response.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <param name="httpRestClientHandler">the current handler instance.</param>
    /// <param name="request">The request to act with. 
    /// The request must be decorated with 
    /// the <see cref="HttpClientAttribute"/> or implements 
    /// the <see cref="IHttpClientAttributeProvider"/> interface.</param>
    /// <returns>Returns a task <see cref="HttpClientResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    public static Task<HttpClientResponse> SendAsync(
        this IHttpClientDispatcher httpRestClientHandler,
        IHttpClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(httpRestClientHandler);
        return httpRestClientHandler
            .SendAsync(request);
    }

    /// <summary>
    /// Sends the request that returns a response 
    /// of <typeparamref name="TResult"/> type.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <param name="httpRestClientHandler">the current handler instance.</param>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="request">The request to act with. 
    /// The request must be decorated with
    /// the <see cref="HttpClientAttribute"/> or implements 
    /// the <see cref="IHttpClientAttributeProvider"/> interface.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <returns>Returns a task <see cref="HttpClientResponse{TResult}"/>
    /// .</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    public static Task<HttpClientResponse<TResult>> SendAsync<TResult>(
        this IHttpClientDispatcher httpRestClientHandler,
        IHttpClientRequest<TResult> request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpRestClientHandler);
        return httpRestClientHandler
            .SendAsync(request, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends the request that returns a response of 
    /// <typeparamref name="TResult"/> type.
    /// Make use of <see langword="using"/> key work when call.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="httpRestClientHandler">the current handler instance.</param>
    /// <param name="request">The request to act with. 
    /// The request must be decorated with
    /// the <see cref="HttpClientAttribute"/> or implements the 
    /// <see cref="IHttpClientAttributeProvider"/> interface.</param>
    /// <returns>Returns a task <see cref="HttpClientResponse{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    public static Task<HttpClientResponse<TResult>> SendAsync<TResult>(
        this IHttpClientDispatcher httpRestClientHandler,
        IHttpClientRequest<TResult> request)
    {
        ArgumentNullException.ThrowIfNull(httpRestClientHandler);
        return httpRestClientHandler
            .SendAsync(request);
    }

    /// <summary>
    /// Determines whether the current exception message is 
    /// <see cref="HttpClientValidation"/>.
    /// The method will try to parse the property named 
    /// 'errors' from the exception message to <see cref="HttpClientValidation"/>.
    /// </summary>
    /// <param name="httpRestClientException">The target exception.</param>
    /// <param name="clientValidation">The <see cref="HttpClientValidation"/>
    /// instance if true.</param>
    /// <param name="exception">The handled exception during process.</param>
    /// <param name="serializerOptions">The optional settings for serializer
    /// .</param>
    /// <returns><see langword="true"/> if exception message is 
    /// <see cref="HttpClientValidation"/>, otherwise <see langword="false"/>
    /// .</returns>
    public static bool IsHttpRestClientValidation(
        this HttpClientException httpRestClientException,
        [MaybeNullWhen(false)] out HttpClientValidation clientValidation,
        [MaybeNullWhen(true)] out Exception exception,
        JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(httpRestClientException);

        serializerOptions ??= new() { PropertyNameCaseInsensitive = true };

        try
        {
            exception = default;
            var anonymousType = new { Errors = default(HttpClientValidation) };
            var result = httpRestClientException
                .Message
                .DeserializeAnonymousType(anonymousType, serializerOptions);

            clientValidation = result?.Errors;
            return clientValidation is not null;
        }
        catch (Exception ex)
            when (ex is JsonException
                    or NotSupportedException
                    or ArgumentNullException)
        {
            exception = ex;
            clientValidation = default;
            return false;
        }
    }

    /// <summary>
    /// Returns an <see cref="IOperationResult"/> 
    /// from the <see cref="HttpClientResponse"/>.
    /// </summary>
    /// <param name="response">The response to act on.</param>
    /// <param name="serializerOptions">The optional 
    /// settings for serializer.</param>
    /// <returns>A bad <see cref="IOperationResult"/></returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="response"/> is null.</exception>
    public static IOperationResult ToOperationResult(
        this HttpClientResponse response,
        JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        serializerOptions ??= new() { PropertyNameCaseInsensitive = true };
        ElementCollection headers = response.Headers.Count > 0
            ? ElementCollection
                .With(response.Headers
                    .ToDictionary()
                    .Select(s => new ElementEntry(s.Key, s.Value))
                    .ToList())
            : [];

        if (response.IsValid)
        {
            return OperationResults
                .Success(response.StatusCode)
                .WithHeaders(headers)
                .Build();
        }

        if (response.IsAnException(out HttpClientException? exception))
        {
            if (exception.IsHttpRestClientValidation(
                out HttpClientValidation? clientValidation,
                out _,
                serializerOptions))
            {
                ElementEntry[] operationErrors = clientValidation
                    .SelectMany(kvp => kvp.Value,
                    (kvp, value) =>
                        new ElementEntry(kvp.Key, kvp.Value.ToArray()))
                    .ToArray();

                return OperationResults
                    .Failure(response.StatusCode)
                    .WithErrors(operationErrors)
                    .WithHeaders(headers)
                    .Build();
            }
            else
            {
                return OperationResults
                    .Failure(response.StatusCode)
                    .WithException(exception)
                    .WithHeaders(headers)
                    .Build();
            }
        }

        return OperationResults
            .Failure(response.StatusCode)
            .WithHeaders(headers)
            .Build();
    }

    /// <summary>
    /// Returns an <see cref="IOperationResult{TValue}"/> 
    /// from the <see cref="HttpClientResponse{TResult}"/>.
    /// If <paramref name="response"/> contains a result and this result 
    /// is <typeparamref name="TValue"/>, the operation will be set with, 
    /// otherwise null.
    /// </summary>
    /// <typeparam name="TValue">The type of the operation content.</typeparam>
    /// <param name="response">The response to act on.</param>
    /// <param name="serializerOptions">The optional settings for 
    /// serializer.</param>
    /// <returns>An <see cref="IOperationResult{TValue}"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="response"/> is null.</exception>
    public static IOperationResult<TValue> ToOperationResult<TValue>(
        this HttpClientResponse<TValue> response,
        JsonSerializerOptions? serializerOptions = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        TValue value = response.Result is TValue result ? result : default!;
        ElementCollection headers = response.Headers.Count > 0
            ? ElementCollection
                .With(response.Headers
                    .ToDictionary()
                    .Select(s => new ElementEntry(s.Key, s.Value))
                    .ToList())
            : [];

        if (response.IsValid)
        {
            return OperationResults
                .Success<TValue>(response.StatusCode)
                .WithResult(value)
                .WithHeaders(headers)
                .Build();
        }

        if (response.IsAnException(out HttpClientException? exception))
        {
            if (exception.IsHttpRestClientValidation(
                out HttpClientValidation? clientValidation,
                out _,
                serializerOptions))
            {
                ElementEntry[] operationErrors = clientValidation
                    .SelectMany(kvp => kvp.Value,
                    (kvp, value) =>
                        new ElementEntry(kvp.Key, kvp.Value.ToArray()))
                    .ToArray();

                return OperationResults
                    .Failure<TValue>(response.StatusCode)
                    .WithErrors(operationErrors)
                    .WithHeaders(headers)
                    .Build();
            }
            else
            {
                return OperationResults
                    .Failure<TValue>(response.StatusCode)
                    .WithException(exception)
                    .WithHeaders(headers)
                    .Build();
            }
        }

        return OperationResults
            .Failure<TValue>(response.StatusCode)
            .WithHeaders(headers)
            .Build();
    }

    internal static IDictionary<string, string> ToDictionary(
        this NameValueCollection nameValueCollection)
    {
        Dictionary<string, string> result = [];
        foreach (string? key in nameValueCollection.AllKeys)
        {
            if (key is not null && nameValueCollection[key] is { } value)
            {
                result.Add(key, value);
            }
        }

        return result;
    }

    internal static string AddQueryString(
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

        // If there is an anchor, then the query string must
        // be inserted before its first occurrence.
        if (anchorIndex != -1)
        {
            anchorText = path[anchorIndex..];
            uriToBeAppended = path[..anchorIndex];
        }

#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
        int queryIndex = uriToBeAppended
            .IndexOf('?', StringComparison.InvariantCulture);
#pragma warning restore CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
        bool hasQuery = queryIndex != -1;

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

    internal static T? DeserializeAnonymousType<T>(
        this string json,
        T _,
        JsonSerializerOptions? options = default)
         => JsonSerializer.Deserialize<T>(json, options);

    internal static async Task<HttpClientException?>
        BuildExceptionAsync(this HttpResponseMessage httpResponse)
        => await httpResponse.Content.ReadAsStringAsync()
            .ConfigureAwait(false) switch
        {
            { } content when !string.IsNullOrWhiteSpace(content)
                => new HttpClientException(content),
            _ => default
        };
}
