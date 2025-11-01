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
namespace Xpandables.Net.Events.Repositories;

/// <summary>
/// Represents an integration for handling entity events with error information.
/// </summary>
/// <remarks>This interface extends <see cref="IEntityEvent"/> to include error handling capabilities.
/// Implementations should provide the error message associated with the event entity, if any.</remarks>
public interface IEntityEventIntegration : IEntityEvent
{
    /// <summary>
    /// Gets the error message associated with the event entity.
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// Gets the number of publish attempts made for the current operation.
    /// </summary>
    int AttemptCount { get; }

    /// <summary>
    /// When the next attempt should be made to process the event.
    /// </summary>
    DateTime? NextAttemptOn { get; }

    /// <summary>
    /// Identifier of the scheduler instance that claims the event for processing.
    /// </summary>
    Guid? ClaimId { get; }
}