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
using System.Text.Json;

namespace System.Net.Http.Rests.Abstractions;

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

    /// <summary>
    /// Creates a new instance of <see cref="RestResponseContext{TResponse}"/> by copying request and response details
    /// from an existing context.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response object contained in the context. Must not be null.</typeparam>
    /// <param name="context">The source <see cref="RestResponseContext"/> from which to copy request and response information. Cannot be
    /// null.</param>
    /// <returns>A new <see cref="RestResponseContext{TResponse}"/> instance initialized with the request, message, and
    /// serializer options from the specified context.</returns>
    public static RestResponseContext<TResponse> Create<TResponse>(RestResponseContext context)
        where TResponse : notnull
    {
        ArgumentNullException.ThrowIfNull(context);

        return new()
        {
            Request = context.Request,
            Message = context.Message,
            SerializerOptions = context.SerializerOptions
        };
    }

    /// <summary>
    /// Creates a new stream context for reading a REST response, using the specified response context and type
    /// parameter.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response object to be deserialized from the stream. Must not be null.</typeparam>
    /// <param name="context">The response context containing the request, message, and serializer options to use for the stream. Cannot be
    /// null.</param>
    /// <returns>A new <see cref="RestResponseStreamContext{TResponse}"/> initialized with the provided context information.</returns>
    public static RestResponseStreamContext<TResponse> CreateStream<TResponse>(RestResponseContext context)
        where TResponse : notnull
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
/// Provides contextual information for a REST response, including access to the originating request and the
/// deserialized result data.
/// </summary>
/// <typeparam name="TResponse">The type of the response data returned by the REST response.</typeparam>
public class RestResponseContext<TResponse> : RestResponseContext
    where TResponse : notnull;

/// <summary>
/// Provides context information for a REST response that includes a streamed payload of type <typeparamref
/// name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response payload contained in the stream. Must not be null.</typeparam>
public class RestResponseStreamContext<TResponse> : RestResponseContext
    where TResponse : notnull;