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
namespace System.Events;

/// <summary>
/// Provides access to the current event context for the executing operation.
/// </summary>
/// <remarks>Use this interface to retrieve contextual information about the event being processed, such as
/// metadata or correlation identifiers. The value of <see cref="Current"/> may be <c>null</c> if no event context is
/// available.</remarks>
public interface IEventContextAccessor
{
    /// <summary>
    /// Gets the current event context associated with the executing operation.
    /// </summary>
    /// <remarks>Use this property to access contextual information about the ongoing event, such as
    /// correlation identifiers or metadata relevant to event processing. The returned context may vary depending on the
    /// execution environment and may be null if no event context is available.</remarks>
    EventContext Current { get; }
}