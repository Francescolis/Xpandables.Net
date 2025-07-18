﻿/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Repositories.Filters;

/// <summary>
/// Represents a filter for event entity integration.
/// </summary>
public sealed record EventFilterIntegration : EventFilter<EntityIntegrationEvent, IIntegrationEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventFilterIntegration"/> class.
    /// </summary>
    /// <remarks>This constructor sets the required members of the <see cref="EventFilterIntegration"/>
    /// class.</remarks>
    [SetsRequiredMembers]
    public EventFilterIntegration() : base() { }
}
