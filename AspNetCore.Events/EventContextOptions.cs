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
namespace Microsoft.AspNetCore;

/// <summary>
/// Represents configuration options for event context propagation, including header names for correlation and causation
/// identifiers.
/// </summary>
/// <remarks>Use this type to specify the HTTP header names used to carry correlation and causation IDs when
/// propagating event context across service boundaries. These options are typically used in distributed systems to
/// enable end-to-end tracing and diagnostics.</remarks>
public sealed record EventContextOptions
{
    /// <summary>
    /// Gets the name of the HTTP header used to transmit the correlation ID for request tracing.
    /// The default value is "traceparent".
    /// </summary>
    public string CorrelationIdHeaderName { get; init; } = "traceparent";

    /// <summary>
    /// Gets the name of the HTTP header used to carry the causation identifier for distributed tracing or correlation
    /// purposes. The default value is "X-Causation-Id".
    /// </summary>
    public string CausationIdHeaderName { get; init; } = "X-Causation-Id";
}
