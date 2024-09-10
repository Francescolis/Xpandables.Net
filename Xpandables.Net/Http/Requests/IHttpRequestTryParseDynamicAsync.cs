
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
using System.Collections;
using System.Reflection;
using System.Text.Json;

using Microsoft.Extensions.Primitives;

using Xpandables.Net.Http;
using Xpandables.Net.Primitives.Converters;

namespace Xpandables.Net.Http.Requests;

/// <summary>
/// For route, request and header custom binding sources 
/// in minimal Api for asynchronous processes, with a dynamic context.
/// </summary>
/// <typeparam name="TRequest">The type of the custom binding parameter
/// .</typeparam>
public interface IHttpRequestTryParseDynamicAsync<TRequest>
{
    /// <summary>
    /// For route binding sources in minimal Api for asynchronous processes.
    /// </summary>
    public interface IRouteRequest
    {
        /// <summary>
        /// The method discovered by <see langword="RequestDelegateFactory"/> 
        /// on types used as parameters of route handler delegates to support 
        /// custom binding.
        /// </summary>
        /// <param name="context">The <see langword="HttpContext"/> 
        /// instance.</param>
        /// <param name="parameter">The <see cref="ParameterInfo"/> 
        /// for the parameter being bound to.</param>
        /// <returns>The value to assign to the parameter.</returns>
        public static Task<TRequest?> BindAsync(
            dynamic context,
            ParameterInfo parameter)
        {
            _ = parameter;
            Dictionary<string, object?> dictionary =
                ((IEnumerable<KeyValuePair<string, object?>>)
                context.Request.RouteValues)
                .ToDictionary(d => d.Key, d => d.Value);

            return DoBindAsync(dictionary);
        }
    }

    /// <summary>
    /// For header binding sources in minimal Api for asynchronous processes.
    /// </summary>
    public interface IHeaderRequest
    {
        /// <summary>
        /// The method discovered by 
        /// <see langword="RequestDelegateFactory"/> on types used as 
        /// parameters of header handler delegates to support custom binding.
        /// </summary>
        /// <param name="context">The <see langword="HttpContext"/> 
        /// instance.</param>
        /// <param name="parameter">The <see cref="ParameterInfo"/> 
        /// for the parameter being bound to.</param>
        /// <returns>The value to assign to the parameter.</returns>
        public static Task<TRequest?> BindAsync(
            dynamic context,
            ParameterInfo parameter)
        {
            _ = parameter;
            Dictionary<string, string?> dictionary =
                ((IEnumerable<KeyValuePair<string, StringValues>>)
                context.Request.Headers)
                .ToDictionary(d => d.Key, d => (string?)d.Value);

            return DoBindAsync(dictionary);
        }
    }

    /// <summary>
    /// For request binding sources in minimal Api for asynchronous processes, 
    /// </summary>
    public interface IQueryRequest
    {
        /// <summary>
        /// The method discovered by 
        /// <see langword="RequestDelegateFactory"/> on types used 
        /// as parameters of request handler delegates to support custom binding.
        /// </summary>
        /// <param name="context">The <see langword="HttpContext"/> 
        /// instance.</param>
        /// <param name="parameter">The <see cref="ParameterInfo"/> 
        /// for the parameter being bound to.</param>
        /// <returns>The value to assign to the parameter.</returns>
        public static Task<TRequest?> BindAsync(
            dynamic context,
            ParameterInfo parameter)
        {
            _ = parameter;
            Dictionary<string, string?> dictionary =
                ((IEnumerable<KeyValuePair<string, StringValues>>)
                context.Request.Query)
                .ToDictionary(d => d.Key, d => (string?)d.Value);

            return DoBindAsync(dictionary);
        }
    }

    /// <summary>
    /// The method discovered by <see langword="RequestDelegateFactory"/> 
    /// on types used as parameters of route handler delegates to support 
    /// custom binding.
    /// </summary>
    /// <param name="context">The <see langword="HttpContext"/> instance.</param>
    /// <param name="parameter">The <see cref="ParameterInfo"/> 
    /// for the parameter being bound to.</param>
    /// <returns>The value to assign to the parameter.</returns>   
    static abstract Task<TRequest?> BindAsync(
        dynamic context,
        ParameterInfo parameter);

    internal static async Task<TRequest?> DoBindAsync(IDictionary dictionary)
    {
        await Task.Yield();

        string jsonString = JsonSerializer
            .Serialize(
                dictionary,
                JsonSerializerDefaultOptions.OptionDefaultWeb);

        TRequest? request = JsonSerializer
            .Deserialize<TRequest>(
                jsonString,
                JsonSerializerDefaultOptions.OptionDefaultWeb);

        return request;
    }
}