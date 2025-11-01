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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Xpandables.Net.Rests;

/// <summary>
/// Represents the context for an HTTP response, including the response message and JSON serializer options.
/// </summary>
public class RestResponseContext
{
    /// <summary>
    /// Represents a required REST request object. It is initialized at the time of object creation.
    /// </summary>
    public required IRestRequest Request { get; init; }

    /// <summary>
    /// Represents an HTTP response message. It is a required property that must be initialized.
    /// </summary>
    public required HttpResponseMessage Message { get; init; }

    /// <summary>
    /// Specifies the options for JSON serialization. It is a required property that must be initialized.
    /// </summary>
    public required JsonSerializerOptions SerializerOptions { get; init; }
}

/// <summary>
/// Provides contextual information for a REST response, including access to the originating request and the
/// deserialized result data.
/// </summary>
/// <typeparam name="TResponse">The type of the response data returned by the REST response.</typeparam>
public class RestResponseContext<TResponse> : RestResponseContext
    where TResponse : notnull
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
    public static RestResponseContext<TResponse> Create(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new()
        {
            Request = context.Request,
            Message = context.Message,
            SerializerOptions = context.SerializerOptions
        };
    }
}

/// <summary>
/// Provides context information for a REST response that includes a streamed payload of type <typeparamref
/// name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response payload contained in the stream. Must not be null.</typeparam>
public class RestResponseStreamContext<TResponse> : RestResponseContext
    where TResponse : notnull
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
    public static RestResponseStreamContext<TResponse> Create(RestResponseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new()
        {
            Request = context.Request,
            Message = context.Message,
            SerializerOptions = context.SerializerOptions
        };
    }
}