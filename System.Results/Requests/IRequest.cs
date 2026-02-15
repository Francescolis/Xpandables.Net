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

namespace System.Results.Requests;

/// <summary>
/// Represents a marker interface for requests that can be dispatched through the mediator pipeline.
/// </summary>
/// <remarks>Implement this interface on request types to make them compatible with the pipeline
/// and mediator infrastructure.</remarks>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface by design for pipeline dispatch.")]
public interface IRequest;

/// <summary>
/// Represents a request that returns a result of the specified type when processed.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the request.</typeparam>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface by design.")]
public interface IRequest<out TResult> : IRequest;

/// <summary>
/// Represents a request that produces a stream of results of the specified type.
/// </summary>
/// <remarks>Use this interface to define requests that yield multiple results over time, such as asynchronous or
/// observable data streams. Implementations may deliver results incrementally rather than all at once.</remarks>
/// <typeparam name="TResult">The type of the elements returned by the stream produced by the request.</typeparam>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface by design.")]
public interface IStreamRequest<out TResult> : IRequest;

/// <summary>
/// Represents a request for retrieving paged results as a stream, where each page yields a result of the specified
/// type.
/// </summary>
/// <remarks>Use this interface to define requests that support streaming paged data, enabling efficient retrieval
/// of large result sets in manageable segments. Implementations may provide asynchronous or synchronous streaming
/// capabilities depending on the underlying data source.</remarks>
/// <typeparam name="TResult">The type of the result returned for each page in the stream.</typeparam>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface by design.")]
public interface IStreamPagedRequest<out TResult> : IRequest;
