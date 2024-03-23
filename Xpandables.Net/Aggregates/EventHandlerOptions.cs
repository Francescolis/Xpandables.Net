/*******************************************************************************
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
********************************************************************************/
namespace Xpandables.Net.Aggregates;

/// <summary>
/// Contains the options for the event handler.
/// </summary>
public sealed record EventHandlerOptions
{
    /// <summary>
    /// Determines whether to consider no event handler as an error.
    /// </summary>
    public bool ConsiderNoEventHandlerAsError { get; init; }

    /// <summary>
    /// Determines whether to consider no notification handler as an error.
    /// </summary>
    public bool ConsiderNoNotificationHandlerAsError { get; init; }
}
