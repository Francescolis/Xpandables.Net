
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
namespace Xpandables.Net.IntegrationEvents;

/// <summary>
/// Defines a marker interface to be used to mark an object to act as an integration event.
/// An integration event is "something that has happened in the past".
/// An integration event is an event that can cause side effects 
/// to other micro-services, Bounded-Contexts or external systems.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Gets When the event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; init; }

    /// <summary>
    /// Gets the integration event identifier.
    /// </summary>
    Guid Id { get; init; }
}
