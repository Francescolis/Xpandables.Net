
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
using System.Text.Json;

namespace Xpandables.Net.Executions.Rests;

/// <summary>
/// Encapsulates the context for a REST request, including attributes, request details, response message, and
/// serialization options.
/// </summary>
/// <typeparam name="TRestRequest">Defines the type of HTTP request being processed, 
/// ensuring it adheres to the required interface.</typeparam>
public sealed class RestRequestContext<TRestRequest>
    where TRestRequest : class, IRestRequest
{
    /// <summary>
    /// Represents a required attribute of type _MapRestAttribute. 
    /// It is initialized at the time of object creation.
    /// </summary>
    public required _RestAttribute Attribute { get; init; }

    /// <summary>
    /// Represents a required HTTP request of type TRestRequest. 
    /// It is initialized at the time of object creation.
    /// </summary>
    public required TRestRequest Request { get; init; }

    /// <summary>
    /// Represents an HTTP request message. It is a required property that must be initialized.
    /// </summary>
    public required HttpRequestMessage Message { get; init; }

    /// <summary>
    /// Specifies the options for JSON serialization. 
    /// It is a required property that must be initialized.
    /// </summary>
    public required JsonSerializerOptions SerializerOptions { get; init; }
}