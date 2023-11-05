
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
using System.Collections;
using System.Reflection;
using System.Text.Json;

using Microsoft.AspNetCore.Http;

using Xpandables.Net.Text;

namespace Xpandables.Net.Http;

/// <summary>
/// For route, query and header binding sources in minimal Api for asynchronous processes, 
/// </summary>
/// <typeparam name="TRequest">The type of the route parameter.</typeparam>
public interface IRequestTryParseAsync<TRequest>
    where TRequest : notnull
{
    /// <summary>
    /// For route binding sources in minimal Api for asynchronous processes.
    /// </summary>
    public interface IRouteRequest
    {
        /// <summary>
        /// The method discovered by <see langword="RequestDelegateFactory"/> on types used as parameters of route
        /// handler delegates to support custom binding.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> instance.</param>
        /// <param name="parameter">The <see cref="ParameterInfo"/> for the parameter being bound to.</param>
        /// <returns>The value to assign to the parameter.</returns>
        public virtual static ValueTask<TRequest?> BindAsync(HttpContext context, ParameterInfo parameter)
        {
            ArgumentNullException.ThrowIfNull(context);

            var dict = context.Request.RouteValues.ToDictionary(d => d.Key, d => d.Value);

            return DoBindAsync(dict);
        }
    }

    /// <summary>
    /// For header binding sources in minimal Api for asynchronous processes.
    /// </summary>
    public interface IHeaderRequest
    {
        /// <summary>
        /// The method discovered by <see langword="RequestDelegateFactory"/> on types used as parameters of header
        /// handler delegates to support custom binding.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> instance.</param>
        /// <param name="parameter">The <see cref="ParameterInfo"/> for the parameter being bound to.</param>
        /// <returns>The value to assign to the parameter.</returns>
        public virtual static ValueTask<TRequest?> BindAsync(HttpContext context, ParameterInfo parameter)
        {
            ArgumentNullException.ThrowIfNull(context);

            var dict = context.Request.Headers.ToDictionary(d => d.Key, d => (string?)d.Value);

            return DoBindAsync(dict);
        }
    }

    /// <summary>
    /// For query binding sources in minimal Api for asynchronous processes, 
    /// </summary>
    public interface IQueryRequest
    {
        /// <summary>
        /// The method discovered by <see langword="RequestDelegateFactory"/> on types used as parameters of query
        /// handler delegates to support custom binding.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> instance.</param>
        /// <param name="parameter">The <see cref="ParameterInfo"/> for the parameter being bound to.</param>
        /// <returns>The value to assign to the parameter.</returns>
        public virtual static ValueTask<TRequest?> BindAsync(HttpContext context, ParameterInfo parameter)
        {
            ArgumentNullException.ThrowIfNull(context);

            var dict = context.Request.Query.ToDictionary(d => d.Key, d => (string?)d.Value);

            return DoBindAsync(dict);
        }
    }

    /// <summary>
    /// The method discovered by <see langword="RequestDelegateFactory"/> on types used as parameters of route/query/header
    /// handler delegates to support custom binding.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance.</param>
    /// <param name="parameter">The <see cref="ParameterInfo"/> for the parameter being bound to.</param>
    /// <returns>The value to assign to the parameter.</returns>
    static abstract ValueTask<TRequest?> BindAsync(HttpContext context, ParameterInfo parameter);

    internal static ValueTask<TRequest?> DoBindAsync(IDictionary dictionary)
    {
        var jsonString = JsonSerializer.Serialize(dictionary, JsonSerializerDefaultOptions.OptionDefaultWeb);

        var request = JsonSerializer.Deserialize<TRequest>(jsonString, JsonSerializerDefaultOptions.OptionDefaultWeb);

        return ValueTask.FromResult(request);
    }
}
