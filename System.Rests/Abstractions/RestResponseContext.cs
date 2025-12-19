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

namespace System.Rests.Abstractions;

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
    /// Creates a new instance of the generic RestResponseContext with the specified request type, copying relevant data
    /// from an existing context.
    /// </summary>
    /// <remarks>Use this method when you need to create a strongly-typed response context from an existing
    /// context with a compatible request type. The method casts the request from the original context to the specified
    /// type parameter.</remarks>
    /// <typeparam name="TRequest">The type of the request to associate with the new context. Must implement IRestRequest and cannot be null.</typeparam>
    /// <param name="context">The existing RestResponseContext instance from which to copy the request, message, and serializer options.
    /// Cannot be null.</param>
    /// <returns>A new RestResponseContext instance with the request cast to the specified type and other properties copied from
    /// the provided context.</returns>
    public static RestResponseContext<TRequest> Create<TRequest>(RestResponseContext context)
        where TRequest : notnull, IRestRequest
    {
        ArgumentNullException.ThrowIfNull(context);

        return new()
        {
            Request = (TRequest)context.Request,
            Message = context.Message,
            SerializerOptions = context.SerializerOptions
        };
    }
}

/// <summary>
/// Provides response context information for a REST operation, including strongly-typed access to the originating
/// request.
/// </summary>
/// <remarks>Use this class to access both generic response context and the specific request that initiated the
/// REST operation. This is useful when handling responses that require information about the original request, such as
/// for logging, error handling, or correlation purposes.</remarks>
/// <typeparam name="TRequest">The type of the REST request associated with the response. Must implement <see cref="IRestRequest"/> and cannot be
/// null.</typeparam>
public class RestResponseContext<TRequest> : RestResponseContext
    where TRequest : notnull, IRestRequest;