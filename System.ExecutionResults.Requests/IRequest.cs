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
namespace System.ExecutionResults;

/// <summary>
/// Represents a request with metadata about its creation time.
/// </summary>
/// <remarks>Implementations of this interface provide access to the creation timestamp of the request, which can
/// be used for auditing, logging, or tracking purposes.</remarks>
public interface IRequest
{
    /// <summary>
    /// Gets the date and time when the request was created.
    /// </summary>
    public DateTime CreatedAt => DateTime.Now;
}

/// <summary>
/// Represents a request that returns a result of the specified type when processed.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the request.</typeparam>
public interface IRequest<out TResult> : IRequest;

/// <summary>
/// Represents a request that produces a stream of results of the specified type.
/// </summary>
/// <remarks>Use this interface to define requests that yield multiple results over time, such as asynchronous or
/// observable data streams. Implementations may deliver results incrementally rather than all at once.</remarks>
/// <typeparam name="TResult">The type of the elements returned by the stream produced by the request.</typeparam>
public interface IStreamRequest<out TResult> : IRequest;
