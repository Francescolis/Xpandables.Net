/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Reflection;

namespace Xpandables.Net.Http;

/// <summary>
/// Provides a mechanism for retrieving the associated RestAttribute for a given REST request, using either attribute
/// decoration or a custom builder implementation.
/// </summary>
/// <remarks>This class supports both requests decorated with RestAttribute and requests that implement
/// IRestAttributeBuilder for dynamic attribute construction. It is typically used to centralize attribute resolution
/// logic in REST client frameworks.</remarks>
/// <param name="serviceProvider">The service provider used to resolve dependencies when building RestAttribute instances.</param>
public sealed class RestAttributeProvider(IServiceProvider serviceProvider) : IRestAttributeProvider
{
    /// <summary>
    /// Retrieves the associated RestAttribute for the specified REST request.
    /// </summary>
    /// <remarks>If the request implements IRestAttributeBuilder, the RestAttribute is obtained by invoking
    /// its Build method. Otherwise, the method attempts to retrieve a RestAttribute applied to the request's type. This
    /// method requires that every request either implements IRestAttributeBuilder or is decorated with a
    /// RestAttribute.</remarks>
    /// <param name="request">The REST request for which to obtain the associated RestAttribute. Cannot be null.</param>
    /// <returns>The RestAttribute associated with the request, either by building it using IRestAttributeBuilder or by
    /// retrieving it from the request's type. Never returns null.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the request is not decorated with a RestAttribute and does not implement IRestAttributeBuilder.</exception>
    public RestAttribute GetRestAttribute(IRestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request is IRestAttributeBuilder builder)
        {
            return builder.Build(serviceProvider);
        }

        return request
            .GetType()
            .GetCustomAttribute<RestAttribute>(true)
            ?? throw new InvalidOperationException(
                $"Request must be decorated with one of the {nameof(RestAttribute)} " +
                $"or implement {nameof(IRestAttributeBuilder)}");
    }
}