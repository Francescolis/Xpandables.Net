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
using System.Runtime.InteropServices;

namespace System.Events.Domain;

/// <summary>
/// Provides contextual information for an event or operation, including correlation and causation identifiers used for
/// tracing and diagnostics.
/// </summary>
/// <remarks>Use this type to propagate correlation and causation identifiers across service or process
/// boundaries. This enables consistent tracking of operations for logging, diagnostics, and distributed tracing
/// scenarios. The identifiers are typically used to relate events and requests in complex, distributed
/// systems.</remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct EventContext
{
    /// <summary>
    /// Gets the correlation identifier associated with the current operation or request.
    /// </summary>
    /// <remarks>Use this property to track or relate operations across system boundaries for diagnostics or
    /// logging purposes. The value is typically propagated between services to enable end-to-end tracing.</remarks>
    public Guid? CorrelationId { get; init; }

    /// <summary>
    /// Gets the identifier that represents the cause of the current operation or event, if available.
    /// </summary>
    /// <remarks>Use this property to track the origin or triggering action of an operation, such as for
    /// distributed tracing or correlation in event-driven systems. The value is typically propagated across service
    /// boundaries to maintain context.</remarks>
    public Guid? CausationId { get; init; }
}
