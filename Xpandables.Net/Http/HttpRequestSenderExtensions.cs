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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions;
using Xpandables.Net.Text;

namespace Xpandables.Net.Http;
/// <summary>
/// Provides extension methods for <see cref="IHttpSender"/>.
/// </summary>
public static class HttpRequestSenderExtensions
{
    /// <summary>
    /// Converts the <see cref="HttpResponseHeaders"/> to a 
    /// <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="response">The response to act on.</param>
    /// <returns>An instance of <see cref="NameValueCollection"/>.</returns>
    public static NameValueCollection ToNameValueCollection(
        this HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return Enumerable
                .Empty<(string Name, string Value)>()
                .Concat(
                    response.Headers
                        .SelectMany(kvp => kvp.Value
                            .Select(v => (Name: kvp.Key, Value: v))
                            )
                        )
                .Concat(
                    response.Content.Headers
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
    /// Converts the <see cref="HttpHeaders"/> to a 
    /// <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="headers">The headers to act on.</param>
    /// <returns>An instance of <see cref="NameValueCollection"/>.</returns>
    public static NameValueCollection ToNameValueCollection(
        this HttpHeaders headers)
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
    /// Determines if the given <see cref="HttpRequestException"/> contains 
    /// validation errors.
    /// </summary>
    /// <param name="clientException">The client exception to check.</param>
    /// <param name="errors">The validation errors if present.</param>
    /// <param name="exception">The exception if deserialization fails.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns><see langword="true"/> if validation errors are present; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsHttpClientValidation(
       this HttpRequestException clientException,
       [MaybeNullWhen(false)] out HttpRequestValidation errors,
       [MaybeNullWhen(true)] out Exception exception,
       JsonSerializerOptions? options = default)
    {
        options ??= DefaultSerializerOptions.Defaults;

        try
        {
            exception = default;
            var anonymousType = new { Errors = default(HttpRequestValidation) };
            var results = clientException.Message
                .DeserializeAnonymousType(anonymousType, options);

            errors = results?.Errors;
            return errors is not null;
        }
        catch (Exception ex)
            when (ex is JsonException
                    or NotSupportedException
                    or ArgumentNullException)
        {
            exception = ex;
            errors = default;
            return false;
        }
    }

    /// <summary>
    /// Converts the <see cref="HttpResponse"/> to an <see cref="ExecutionResult"/>.
    /// </summary>
    /// <param name="response">The HTTP client response to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>An instance of <see cref="ExecutionResult"/>.</returns>
    public static ExecutionResult ToExecutionResult(
        this HttpResponse response,
        JsonSerializerOptions? options = default)
    {
        options ??= DefaultSerializerOptions.Defaults;

        ElementCollection headers = response.Headers.ToElementCollection();

        if (response.IsSuccessStatusCode)
        {
            return ExecutionResults
                .Success(response.StatusCode)
                .WithHeaders(headers)
                .Build();
        }

        if (response.Exception is null)
        {
            return ExecutionResults
                .Failure(response.StatusCode)
                .WithHeaders(headers)
                .Build();
        }

        HttpRequestException responseException = response.Exception;

        if (responseException.IsHttpClientValidation(
            out HttpRequestValidation? errors,
            out _,
            options))
        {
            return ExecutionResults
                .Failure(response.StatusCode)
                .WithHeaders(headers)
                .WithErrors(errors.ToElementCollection())
                .Build();
        }

        return ExecutionResults
            .Failure(response.StatusCode)
            .WithHeaders(headers)
            .WithException(responseException)
            .Build();
    }

    /// <summary>
    /// Converts the <see cref="HttpResponse"/> to an <see cref="ExecutionResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="response">The HTTP client response to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>An instance of <see cref="ExecutionResult"/>.</returns>
    public static ExecutionResult<TResult> ToExecutionResult<TResult>(
        this ResponseHttp<TResult> response,
        JsonSerializerOptions? options = default)
    {
        options ??= DefaultSerializerOptions.Defaults;
        TResult? result = response.Result is TResult value ? value : default;
        ElementCollection headers = response.Headers.ToElementCollection();

        if (response.IsSuccessStatusCode)
        {
            IExecutionResultSuccessBuilder<TResult> successBuilder = ExecutionResults
                .Success<TResult>(response.StatusCode)
                .WithHeaders(headers);

            return (result is not null) switch
            {
                true => successBuilder.WithResult(result).Build(),
                false => successBuilder.Build()
            };
        }

        if (response.Exception is null)
        {
            return ExecutionResults
                .Failure<TResult>(response.StatusCode)
                .WithHeaders(headers)
                .Build();
        }

        HttpRequestException responseException = response.Exception;

        if (responseException.IsHttpClientValidation(
            out HttpRequestValidation? errors,
            out _,
            options))
        {
            return ExecutionResults
                .Failure<TResult>(response.StatusCode)
                .WithHeaders(headers)
                .WithErrors(errors.ToElementCollection())
                .Build();
        }

        return ExecutionResults
            .Failure<TResult>(response.StatusCode)
            .WithHeaders(headers)
            .WithException(responseException)
            .Build();
    }


    /// <summary>
    /// Builds an <see cref="HttpRequestException"/> asynchronously from the 
    /// <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="httpResponse">The HTTP response message.</param>
    /// <returns>An instance of <see cref="HttpRequestException"/> if the content
    /// is not empty; otherwise, null.</returns>
    internal static async Task<HttpRequestException?>
        BuildExceptionAsync(this HttpResponseMessage httpResponse)
        => await httpResponse.Content.ReadAsStringAsync()
            .ConfigureAwait(false) switch
        {
            { } content when !string.IsNullOrWhiteSpace(content)
                => new HttpRequestException(content),
            _ => default
        };

    internal static ElementCollection ToElementCollection(
        this NameValueCollection headers) =>
        headers.Count > 0
            ? ElementCollection.With(
                [.. headers
                .ToDictionary()
                .Select(x => new ElementEntry(x.Key, [x.Value]))])
            : [];

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

    internal static ElementCollection ToElementCollection(
        this IDictionary<string, IEnumerable<string>> dictionary)
        => ElementCollection.With([.. dictionary.Select(x => new ElementEntry(x.Key, [.. x.Value]))]);

    internal static T? DeserializeAnonymousType<T>(
        this string json,
        T _,
        JsonSerializerOptions? options = default)
         => JsonSerializer.Deserialize<T>(json, options);
}
