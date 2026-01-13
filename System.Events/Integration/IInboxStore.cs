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
namespace System.Events.Integration;

/// <summary>
/// Represents an event that failed to be processed in the inbox, including its identifier, the consumer that attempted
/// processing, and the associated error message.
/// </summary>
/// <param name="EventId">The unique identifier of the event that failed.</param>
/// <param name="Consumer">The name of the consumer that attempted to process the event.</param>
/// <param name="Error">A description of the error that occurred during event processing.</param>
public readonly record struct FailedInboxEvent(Guid EventId, string Consumer, string Error);

/// <summary>
/// Represents a contract for storing and retrieving inbox messages.
/// </summary>
/// <remarks>Implementations of this interface provide mechanisms for persisting and accessing messages in an
/// inbox. The specific storage medium and retrieval strategies depend on the concrete implementation.</remarks>
public interface IInboxStore
{

}
