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
using System.Reflection;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions;
using Xpandables.Net.Rests;

namespace Xpandables.Net.Rests;

/// <summary>
/// Provides extension methods for the <see cref="IRestClient"/> class.
/// </summary>
public static class RestClientExtensions
{
    private static readonly MethodInfo ToResponseResultMethod =
        typeof(RestClientExtensions).GetMethod(nameof(ToRestResponse),
            BindingFlags.Static | BindingFlags.Public,
            [typeof(RestResponse)])!;

    /// <summary>
    /// Converts a RestResponse to a RestResponse of a specified type.
    /// </summary>
    /// <typeparam name="TResult">Specifies the type of the data contained in the response.</typeparam>
    /// <param name="response">Represents the original response to be converted.</param>
    /// <returns>Returns the original response cast to the specified type.</returns>
    public static RestResponse<TResult> ToRestResponse<TResult>(this RestResponse response) => response;

    /// <summary>
    /// Converts a RestResponse to a specified generic type if applicable. If the type is not generic, it returns the
    /// original response.
    /// </summary>
    /// <param name="response">The response object that contains the data to be converted.</param>
    /// <param name="genericType">The type to which the response should be converted, which must be specified.</param>
    /// <returns>Returns the converted response or the original response if the specified type is generic.</returns>
    public static dynamic ToRestResponse(this RestResponse response, Type genericType)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(genericType);
        if (genericType.IsGenericType)
        {
            return response;
        }

        MethodInfo method = ToResponseResultMethod.MakeGenericMethod(genericType);
        return method.Invoke(null, [response])!;
    }

    /// <summary>
    /// Converts a RestResponse into an ExecutionResult, handling both success and failure cases. It merges headers and
    /// includes relevant status information.
    /// </summary>
    /// <param name="response">The input object containing the response details and status information.</param>
    /// <returns>An ExecutionResult object representing the outcome of the response, including success or failure details.</returns>
    public static ExecutionResult ToExecutionResult(this RestResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        // ReSharper disable once InvertIf
        if (response.IsFailure)
        {
            ExecutionResult executionResult = response.Exception
                .ToExecutionResult(response.StatusCode, response.ReasonPhrase);

            executionResult.Headers.Merge(response.Headers);

            return executionResult;
        }

        return ExecutionResult
            .Success(response.StatusCode)
            .WithHeaders(response.Headers)
            .WithResult(response.Result)
            .Build();
    }

    /// <summary>
    /// Converts HTTP headers into an ElementCollection format.
    /// </summary>
    /// <param name="headers">The HTTP headers to be converted into a collection.</param>
    /// <returns>An ElementCollection containing the headers and their values.</returns>
    public static ElementCollection ToElementCollection(this HttpHeaders headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        ElementCollection collection = [];
        foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
        {
            collection.Add(header.Key, [.. header.Value]);
        }

        return collection;
    }

    internal static async Task<Exception> ToException(this HttpResponseMessage message)
    {
        string content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
        return message.StatusCode.GetAppropriateException(content);
    }
}
