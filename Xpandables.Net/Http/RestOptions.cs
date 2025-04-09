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
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xpandables.Net.Text;

namespace Xpandables.Net.Http;
/// <summary>
/// Represents the options for to manage <see cref="IRestResponseHandler"/>
/// and <see cref="IRestResponseHandler"/> and its associated services.
/// </summary>
public sealed record RestOptions
{
    /// <summary>
    /// Gets the resolver function for resolving types.
    /// </summary>
    public Func<Type, object?>? Resolver { get; internal set; }

    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> to be used.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; }
        = DefaultSerializerOptions.Defaults;

    /// <summary>  
    /// Gets the map request for the specified request.  
    /// </summary>  
    /// <param name="request">The HTTP client request.</param>  
    /// <returns>The map request attribute for the specified request.</returns>  
    /// <exception cref="InvalidOperationException">Thrown when the request is 
    /// not decorated with <see cref="RestAttribute"/> or 
    /// does not implement <see cref="IRestProvider"/>.</exception>  
    public _RestAttribute GetMapRestAttribute(IRestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request is IRestProvider builder
            ? builder.Build(this)
            : request
                .GetType()
                .GetCustomAttribute<_RestAttribute>(true)
                ?? throw new InvalidOperationException(
                    $"Request must be decorated with one of the {nameof(RestAttribute)} " +
                    $"or implement {nameof(IRestProvider)}");
    }

    /// <summary>
    /// Configures the default HTTP client options.
    /// </summary>
    /// <param name="options">The HTTP client options.</param>
    public static void Default(RestOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }

    /// <summary>  
    /// Gets the default HTTP request options.  
    /// </summary>  
    public static RestOptions DefaultRestOptions
    {
        get
        {
            RestOptions options = new();
            Default(options);
            return options;
        }
    }
}
