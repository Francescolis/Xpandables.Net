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

namespace Xpandables.Net.Http;

/// <summary>
/// Represents the context for an HTTP response, including the response message and JSON serializer options.
/// </summary>
public sealed class RestResponseContext
{
    /// <summary>
    /// Represents an HTTP response message. It is a required property that must be initialized.
    /// </summary>
    public required HttpResponseMessage Message { get; init; }

    /// <summary>
    /// Specifies the options for JSON serialization. It is a required property that must be initialized.
    /// </summary>
    public required JsonSerializerOptions SerializerOptions { get; init; }
}
