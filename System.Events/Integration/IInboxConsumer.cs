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
/// Provides a consumer identifier for inbox idempotency when handling integration events.
/// </summary>
/// <remarks>
/// Implement on <see cref="Events.IEventHandler{TEvent}"/> instances to override the default consumer name
/// (handler type name) used as part of the inbox idempotency key.
/// </remarks>
public interface IInboxConsumer
{
    /// <summary>
    /// Gets the logical consumer/handler name for the inbox idempotency key.
    /// </summary>
    string Consumer { get; }
}
